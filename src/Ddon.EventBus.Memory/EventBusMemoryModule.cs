using Ddon.Core;
using Ddon.EventBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.EventBus.MemoryQueue
{
    public class EventBusMemoryModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);
            Load<EventBusModule>(services, configuration);

            services.AddTransient<IEventBus, InMemoryEventBus>();
        }
    }
}
