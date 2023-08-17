using Ddon.Core;
using Ddon.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Identity
{
    public class DomainModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);
            Load<EventBusModule>(services, configuration);
        }
    }
}
