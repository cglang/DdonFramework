using Ddon.Cache;
using Ddon.Cache.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 注册内存缓存相关服务的扩展方法
    /// </summary>
    public static class ServiceCollectionMemoryCacheExtensions
    {
        public static void AddMemoryCache(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICache, MemoryCache>();
        }
    }
}
