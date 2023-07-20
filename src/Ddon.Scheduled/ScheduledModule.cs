using System;
using Ddon.Core;
using Ddon.Core.Use.Reflection;
using Ddon.EventBus.Memory;
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

        var implementTypes = AssemblyHelper.FindImplementType(typeof(IScheduled), AppDomain.CurrentDomain.GetAssemblies());

        foreach (var implementType in implementTypes)
        {
            services.AddScoped(implementType);
        }
    }
}
