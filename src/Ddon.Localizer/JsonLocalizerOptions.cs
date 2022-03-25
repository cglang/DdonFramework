using Microsoft.Extensions.Caching.Distributed;

namespace Ddon.Localizer
{
    /// <summary>
    /// Configuration options for the json localizer
    /// json 本地化程序的配置选项
    /// </summary>
    public class JsonLocalizerOptions
    {
        public JsonLocalizerOptions()
        {
            ResourcesPath = "Localization";
            CacheKeyPrefix = "__localization__";
        }

        public JsonLocalizerOptions(string resourcesPath, string cacheKeyPrefix)
        {
            ResourcesPath = resourcesPath;
            CacheKeyPrefix = cacheKeyPrefix;
        }

        /// <summary>
        /// The path of the json files where all the localization is stored
        /// 存储所有本地化的 json 文件的路径
        /// </summary>
        public string ResourcesPath { get; set; }

        /// <summary>
        /// The cache key prefix to use when creating cache
        /// entries in the <see cref="IDistributedCache"/> implementation
        /// 在 <see cref="IDistributedCache"/> 实现中创建缓存条目时使用的缓存键前缀
        /// </summary>
        public string CacheKeyPrefix { get; set; }
    }
}
