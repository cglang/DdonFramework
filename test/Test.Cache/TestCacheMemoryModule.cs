using Ddon.Cache;
using Ddon.Cache.Memory;
using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Cache
{
    public class TestCacheMemoryModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CacheMemoryModule>(services, configuration);
        }
    }
}
