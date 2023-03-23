using System;
using System.Linq;
using Ddon.Core;
using Ddon.EventBus.MemoryQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Scheduled;

public class ScheduledModule : Module
{
    public override void Load(IServiceCollection services, IConfiguration configuration)
    {
        Load<EventBusMemoryModule>(services, configuration);
        services.AddHostedService<ScheduledHostService>();
        services.AddScoped<ScheduledService>();

        var baseType = typeof(IScheduled);

        var implementTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(types => types.GetTypes())
            .Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.IsClass).ToList();

        implementTypes.ForEach(implementType =>
        {
            services.AddScoped(implementType);
        });
    }
}
