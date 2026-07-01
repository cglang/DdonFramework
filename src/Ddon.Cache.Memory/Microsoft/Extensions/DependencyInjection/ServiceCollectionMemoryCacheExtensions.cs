using Ddon.Cache;
using Ddon.Cache.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionMemoryCacheExtensions
    {
        public static IServiceCollection AddMemoryCache(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICache, MemoryCache>();

            return services;
        }
    }
}
