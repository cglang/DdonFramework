using Ddon.Core;
using Ddon.EventBus.MemoryQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job
{
    public class JobModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<EventBusMemoryModule>(services, configuration);

            services.AddSingleton<IMissionManager, MissionManager>();
        }
    }
}
