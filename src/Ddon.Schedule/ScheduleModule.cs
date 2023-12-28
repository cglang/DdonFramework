using System;
using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Schedule;

public class ScheduleModule : Module<ScheduleServiceConfiguration>
{
    public override void Load(
        IServiceCollection services,
        IConfiguration configuration,
        Action<ScheduleServiceConfiguration>? optionAction)
    {
        Load<CoreModule>(services, configuration);
        services.AddHostedService<ScheduleHostService>();
        services.AddScoped<ScheduleService>();

        services.AddScoped<IScheduleManager, ScheduleManager>();

        services.Configure<ScheduleOptions>(configuration.GetSection("Schedule"));

        var scheduleServiceConfiguration = new ScheduleServiceConfiguration(services);
        optionAction?.Invoke(scheduleServiceConfiguration);
    }
}
