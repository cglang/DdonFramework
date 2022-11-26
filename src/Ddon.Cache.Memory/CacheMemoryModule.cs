using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Cache.Memory
{
    public class CacheMemoryModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CacheModule>(services, configuration);
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICache, Cache>();
        }
    }
}
