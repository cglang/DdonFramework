using Ddon.Cache.Redis;
using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Cache
{
    public class CacheRedisModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CacheModule>(services, configuration);

            var cacheRedisOptions = configuration.GetSection(nameof(CacheRedisOptions)).Get<CacheRedisOptions>();
            if (cacheRedisOptions is null) throw new("找不到配置信息");

            services.AddStackExchangeRedisCache(options => { options = cacheRedisOptions; });
        }
    }
}
