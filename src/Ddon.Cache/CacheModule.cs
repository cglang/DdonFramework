using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Cache
{
    public class CacheModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);
        }
    }
}
