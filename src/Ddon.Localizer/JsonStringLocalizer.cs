using Ddon.Cache;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

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

            var fullFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.Value.ResourcesPath, $"{CultureInfo.CurrentCulture.Name}.json");
            LoadLocalizer(fullFilePath).Wait();
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
            var cacheKey = $"{_options.Value.CacheKeyPrefix}_{key}";
            var cacheValue = await _cache.GetAsync<string>(cacheKey);
            if (!string.IsNullOrEmpty(cacheValue)) return cacheValue;

            return default;
        }

        public async Task LoadLocalizer(string fullFilePath)
        {
            using var streamReader = new StreamReader(fullFilePath);
            var json = await streamReader.ReadToEndAsync();
            var lkvs = await PullDeserialize(json);

            Parallel.ForEach(lkvs, lkv =>
            {
                _cache.SetAsync($"{_options.Value.CacheKeyPrefix}_{lkv.Key}", lkv.Value);
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