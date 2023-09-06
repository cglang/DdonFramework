using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ddon.Schedule;

/// <summary>
/// Job 服务启动
/// </summary>
internal class ScheduleHostService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ScheduleOptions _options;

    /// <summary>
    /// Job 服务启动
    /// </summary>
    public ScheduleHostService(
        IServiceProvider serviceProvider,
        IOptions<ScheduleOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enable) return;

        var baseType = typeof(ISchedules);

        var implementTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(types => types.GetTypes())
            .Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.IsClass)
            .ToList();

        var schedules = from implementType in implementTypes
                        where !string.IsNullOrWhiteSpace(implementType.FullName)
                        from method in implementType.GetMethods()
                        let attrs = method.GetCustomAttributes(typeof(CronAttribute), false).As<IEnumerable<CronAttribute>>()
                        from attr in attrs
                        where attr.Enable
                        let cron = CronExpression.Parse(attr.CronExpression, attr.Format)
                        select new ScheduleInvokeData(cron, TimeZoneInfo.Local, attr.Inclusive, implementType.FullName!, method.Name);
        foreach (var jobInvokeData in schedules)
            ScheduleData.Schedules.Add(Guid.NewGuid(), jobInvokeData);

        schedules = ScheduleInvokeData.GetPathSchedule(_options.SchedulePath);
        foreach (var jobInvokeData in schedules)
            ScheduleData.Schedules.Add(Guid.NewGuid(), jobInvokeData);

        await using var scope = _serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ScheduleService>().StartAsync(stoppingToken);
    }
}
