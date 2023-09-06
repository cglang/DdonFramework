using System;
using Ddon.Core.Use.Cronos;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Schedule;

public class ScheduleServiceConfiguration
{
    private readonly IServiceCollection _services;

    public ScheduleServiceConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    public void Add<TSchedule>(
        string cronExpression,
        CronFormat format = CronFormat.IncludeSeconds,
        bool inclusive = false,
        bool enable = true)
        where TSchedule : class, ISchedule
    {
        if (enable)
        {
            var cron = CronExpression.Parse(cronExpression, format);
            var data = new ScheduleInvokeData(cron, TimeZoneInfo.Local, inclusive, typeof(TSchedule));
            ScheduleData.Schedules.Add(Guid.NewGuid(), data);
            _services.AddScoped<TSchedule>();
        }
    }
}
