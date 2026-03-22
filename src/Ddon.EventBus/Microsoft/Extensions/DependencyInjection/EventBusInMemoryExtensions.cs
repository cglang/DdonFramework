using System.Reflection;
using Ddon.EventBus;
using Ddon.EventBus.Contracts;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusInMemoryExtensions
    {
        public static void AddEventBus(this IServiceCollection services, Assembly assembly)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            services.AddTransient<IEventBus, InMemoryEventBus>();

            services.AddSingleton<BackgroundEventProcessor>();
        }
    }
}
