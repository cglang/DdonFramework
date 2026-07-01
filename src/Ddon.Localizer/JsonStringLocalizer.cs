using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Ddon.Localizer
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();

        private readonly IOptions<JsonLocalizerOptions> _options;

        public JsonStringLocalizer(IOptions<JsonLocalizerOptions> options)
        {
            _options = options;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var culture = CultureInfo.CurrentCulture.Name;
                var dictionary = _cache.GetOrAdd(culture, _ => LoadDictionary(culture));

                if (dictionary.TryGetValue(name, out var value))
                    return new LocalizedString(name, value, false);

                return new LocalizedString(name, $"[{name}]", true);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var entry = this[name];
                return entry.ResourceNotFound
                    ? entry
                    : new LocalizedString(name, string.Format(entry.Value, arguments), false);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentCulture.Name;
            var dictionary = _cache.GetOrAdd(culture, _ => LoadDictionary(culture));

            foreach (var kv in dictionary)
                yield return new LocalizedString(kv.Key, kv.Value, false);
        }

        private Dictionary<string, string> LoadDictionary(string culture)
        {
            var fullPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                _options.Value.ResourcesPath,
                $"{culture}.json");

            if (!File.Exists(fullPath))
                return new Dictionary<string, string>();

            var json = File.ReadAllText(fullPath);
            return FlattenJson(json);
        }

        private static Dictionary<string, string> FlattenJson(string json, string prefix = "")
        {
            var result = new Dictionary<string, string>();
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (dict is null) return result;

            foreach (var kv in dict)
            {
                var key = string.IsNullOrEmpty(prefix) ? kv.Key : $"{prefix}{kv.Key}";

                switch (kv.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                        var nested = FlattenJson(kv.Value.GetRawText(), $"{key}:");
                        foreach (var n in nested)
                            result[n.Key] = n.Value;
                        break;
                    default:
                        result[key] = kv.Value.ToString();
                        break;
                }
            }

            return result;
        }
    }
}
