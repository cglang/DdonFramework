using System;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ScheduleService>().StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enable) return;

        //var baseType = typeof(ISchedule);

        //var implementTypes = AppDomain.CurrentDomain.GetAssemblies()
        //    .SelectMany(types => types.GetTypes())
        //    .Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.IsClass)
        //    .ToList();

        //var jobInvokeDatas = from implementType in implementTypes
        //                     where !string.IsNullOrWhiteSpace(implementType.FullName)
        //                     from method in implementType.GetMethods()
        //                     let attrs = method.GetCustomAttributes(typeof(CronAttribute), false).As<IEnumerable<CronAttribute>>()
        //                     from attr in attrs
        //                     where attr.Enable
        //                     let cron = CronExpression.Parse(attr.CronExpression, attr.Format)
        //                     select new ScheduleInvokeData(cron, attr.Zone, attr.Inclusive, implementType.FullName!, method.Name);
        //foreach (var jobInvokeData in jobInvokeDatas)
        //{
        //    ScheduleData.Schedules.Add(Guid.NewGuid(), jobInvokeData);
        //}

        await using var scope = _serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ScheduleService>().StartAsync(stoppingToken);
    }
}
