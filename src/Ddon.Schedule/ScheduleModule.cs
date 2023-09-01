using System;
using Ddon.Core;
using Ddon.Core.Use.Reflection;
using Ddon.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Schedule;

public class ScheduleModule : Module
{
    public override void Load(IServiceCollection services, IConfiguration configuration)
    {
        Load<EventBusModule>(services, configuration);
        services.AddHostedService<ScheduleHostService>();
        services.AddScoped<ScheduleService>();

        var implementTypes = AssemblyHelper.FindImplementType(typeof(ISchedule), AppDomain.CurrentDomain.GetAssemblies());

        foreach (var implementType in implementTypes)
        {
            services.AddScoped(implementType);
        }
        
        services.Configure<ScheduleOptions>(configuration.GetSection("Schedule"));
    }
}
