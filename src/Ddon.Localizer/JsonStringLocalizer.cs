﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ddon.Cache;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Ddon.Localizer
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ICache _cache;
        private readonly IOptions<JsonLocalizerOptions> _options;

        private readonly Dictionary<string, string> Pairs = new();

        public JsonStringLocalizer(ICache cache, IOptions<JsonLocalizerOptions> options)
        {
            _cache = cache;
            _options = options;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var task = GetStringAsync(name);
                task.Wait();
                var value = task.Result;
                return new LocalizedString(name, value ?? $"[{name}]", value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var actualValue = this[name];
                return !actualValue.ResourceNotFound
                    ? new LocalizedString(name, string.Format(actualValue.Value, arguments), false)
                    : actualValue;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            foreach (var lkv in Pairs)
            {
                yield return new LocalizedString($"{lkv.Key}", lkv.Value, false);
            }
        }

        private async Task<string?> GetStringAsync(string key)
        {
            var prefixKey = new StringBuilder(_options.Value.CacheKeyPrefix).Append('_').Append(CultureInfo.CurrentCulture.Name);

            var languageCacheValue = await _cache.GetAsync<bool>(prefixKey.ToString());
            if (!languageCacheValue) await LoadLocalizer();

            return await _cache.GetAsync<string>(prefixKey.Append($"_{key}").ToString());
        }

        private async Task LoadLocalizer()
        {
            var fullFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.Value.ResourcesPath, $"{CultureInfo.CurrentCulture.Name}.json");
            if (!File.Exists(fullFilePath)) return;

            var json = await File.ReadAllTextAsync(fullFilePath);
            var lkvs = await PullDeserialize(json);

            await _cache.SetAsync($"{_options.Value.CacheKeyPrefix}_{CultureInfo.CurrentCulture.Name}", true);
            Parallel.ForEach(lkvs, async lkv =>
            {
                await _cache.SetAsync($"{_options.Value.CacheKeyPrefix}_{CultureInfo.CurrentCulture.Name}_{lkv.Key}", lkv.Value);
            });
        }

        private async Task<Dictionary<string, string>> PullDeserialize(string json, string baseKey = "", Dictionary<string, string>? pairs = null)
        {
            if (pairs == null) pairs = new Dictionary<string, string>();
            var dics = JsonSerializer.Deserialize<IDictionary<string, object?>>(json)!;
            foreach (var dic in dics)
            {
                var jsonEliment = (JsonElement?)dic.Value;
                if (jsonEliment != null)
                {
                    var kind = jsonEliment.Value.ValueKind;
                    if (kind == JsonValueKind.Object)
                    {
                        await PullDeserialize($"{dic.Value}", $"{baseKey}{dic.Key}:", pairs);
                    }
                    else if (kind == JsonValueKind.Array)
                    {
                        // Array 需要特殊处理
                        pairs.Add($"{baseKey}{dic.Key}", dic.Value?.ToString() ?? string.Empty);
                    }
                    else
                    {
                        pairs.Add($"{baseKey}{dic.Key}", dic.Value?.ToString() ?? string.Empty);
                    }
                }
            }
            return pairs;
        }
    }
}
