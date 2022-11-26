using Ddon.Cache.Redis;
using Ddon.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Cache
{
    public class TestRedisMemoryModule : Module
    {
        public override void Load(IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Load<CacheRedisModule>(services, configuration);
        }
    }
}
