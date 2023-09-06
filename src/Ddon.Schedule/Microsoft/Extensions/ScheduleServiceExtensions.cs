using System;
using Ddon.Schedule;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ScheduleServiceExtensions
{
    public static IServiceCollection SetScheduleService(
        this IServiceCollection services,
        Action<ScheduleServiceConfiguration> options)
    {
        var ScheduleServiceConfiguration = new ScheduleServiceConfiguration(services);
        options.Invoke(ScheduleServiceConfiguration);
        return services;
    }
}
