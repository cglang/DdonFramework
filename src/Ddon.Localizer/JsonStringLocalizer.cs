using Ddon.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Ddon.Localizer
{
    /// <summary>
    /// Service that contains LocalizedStrings with json files as localization
    /// resources and a DistributedCache to cache out the values read from 
    /// json resource files
    /// 包含 LocalizedStrings 和 json 文件作为本地化资源和一个 DistributedCache 的服务来缓存从 json 资源文件中读取的值
    /// </summary>
    public class JsonStringLocalizer : IStringLocalizer
    {
        #region Dependency Injection Fields

        private readonly ICache _cache;
        private readonly IOptions<JsonLocalizerOptions> _options;

        #endregion

        #region Private Fields

        private readonly JsonSerializer _serializer = new JsonSerializer();

        #endregion

        #region Constructors

        /// <summary>
        /// The default constructor for this servce injecting
        /// the required dependencies
        /// 此服务的默认构造函数注入所需的依赖项
        /// </summary>
        /// <param name="cache">The <see cref="IDistributedCache"/> implementation to use</param>
        /// <param name="options">The configuration options for the json localizer</param>
        public JsonStringLocalizer(ICache cache, IOptions<JsonLocalizerOptions> options)
        {
            _cache = cache;
            _options = options;
        }

        #endregion

        #region IStringLocalizer Implementation

        /// <summary>
        /// Gets the string resource with the given name.
        /// 获取具有给定名称的字符串资源。
        /// </summary>
        /// <param name="name">The name of the string resource.</param>
        /// <returns>
        /// The string resource as a <see cref="T:Microsoft.Extensions.Localization.LocalizedString" />.
        /// </returns>
        public LocalizedString this[string name]
        {
            get
            {
                var value = GetString(name);
                return new LocalizedString(name, value ?? $"[{name}]", value == null);
            }
        }

        /// <summary>
        /// Gets the string resource with the given name and formatted with the supplied arguments.
        /// 获取具有给定名称并使用提供的参数进行格式化的字符串资源。
        /// </summary>
        /// <param name="name">The name of the string resource.</param>
        /// <param name="arguments">The values to format the string with.</param>
        /// <returns>
        /// The formatted string resource as a <see cref="T:Microsoft.Extensions.Localization.LocalizedString" />.
        /// </returns>
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

        /// <summary>
        /// Gets all string resources.
        /// 获取所有字符串资源。
        /// </summary>
        /// <param name="includeParentCultures">
        /// A <see cref="T:System.Boolean" /> indicating whether to include strings from parent cultures.
        /// </param>
        /// <returns>The strings.</returns>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.Value.ResourcesPath, $"{CultureInfo.CurrentCulture.Name}.json");
            using (var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sReader = new StreamReader(str))
            using (var reader = new JsonTextReader(sReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                        continue;

                    var key = (string)reader.Value;
                    reader.Read();
                    var value = _serializer.Deserialize<string>(reader);
                    yield return new LocalizedString(key, value, false);
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Extensions.Localization.IStringLocalizer" /> for 
        /// a specific <see cref="T:System.Globalization.CultureInfo" />.
        /// 为特定的 <see cref="T:System.Globalization.CultureInfo" /> 创建一个新的 <see cref="T:Microsoft.Extensions.Localization.IStringLocalizer" />。
        /// </summary>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use.</param>
        /// <returns>
        /// A culture-specific <see cref="T:Microsoft.Extensions.Localization.IStringLocalizer" />.
        /// </returns>
        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            return new JsonStringLocalizer(_cache, _options);
        }

        #endregion

        #region Private Helper Methods

        private string? GetString(string key)
        {
            //var relativeFilePath = $"{_options.Value.ResourcesPath}/{CultureInfo.CurrentCulture.Name}.json";
            var fullFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.Value.ResourcesPath, $"{CultureInfo.CurrentCulture.Name}.json");
            //var fullFilePath = Path.GetFullPath(relativeFilePath);

            if (File.Exists(fullFilePath))
            {
                var cacheKey = $"{_options.Value.CacheKeyPrefix}_{key}";
                var cacheValue = _cache.Get<string>(cacheKey);
                if (!string.IsNullOrEmpty(cacheValue)) return cacheValue;

                var result = PullDeserialize<string>(key, fullFilePath);
                if (!string.IsNullOrEmpty(result))
                    _cache.Set(cacheKey, result);

                return result;
            }

            WriteEmptyKeys(new CultureInfo("en-US"), fullFilePath);
            return default;
        }

        private void WriteEmptyKeys(CultureInfo sourceCulture, string fullFilePath)
        {
            //var sourceFilePath = $"{_options.Value.ResourcesPath}/{sourceCulture.Name}.json";
            var sourceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.Value.ResourcesPath, $"{sourceCulture.Name}.json");

            using (var str = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var outStream = File.Create(fullFilePath))
            using (var sWriter = new StreamWriter(outStream))
            using (var writer = new JsonTextWriter(sWriter))
            using (var sReader = new StreamReader(str))
            using (var reader = new JsonTextReader(sReader))
            {
                writer.Formatting = Formatting.Indented;
                var jobj = JObject.Load(reader);
                writer.WriteStartObject();
                foreach (var property in jobj.Properties())
                {
                    writer.WritePropertyName(property.Name);
                    writer.WriteNull();
                }
                writer.WriteEndObject();
            }
        }

        /// <summary>
        /// This is used to deserialize only one specific value from
        /// the json without loading the entire object.
        /// </summary>
        /// <typeparam name="T">Type of the object to deserialize</typeparam>
        /// <param name="propertyName">Name of the property to get from json</param>
        /// <param name="filePath">The file path of the json resource file</param>
        /// <returns>Deserialized propert from the json</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private T? PullDeserialize<T>(string propertyName, string filePath)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            using (var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sReader = new StreamReader(str))
            using (var reader = new JsonTextReader(sReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName
                        && (string)reader.Value == propertyName)
                    {
                        reader.Read();
                        return _serializer.Deserialize<T>(reader);
                    }
                }
                return default;
            }
        }

        #endregion
    }
}