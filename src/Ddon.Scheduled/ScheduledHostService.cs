using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ddon.Scheduled;

/// <summary>
/// Job 服务启动
/// </summary>
internal class ScheduledHostService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Job 服务启动
    /// </summary>
    public ScheduledHostService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var baseType = typeof(IScheduled);

        var implementTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(types => types.GetTypes())
            .Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.IsClass)
            .ToList();

        var jobInvokeDatas = from implementType in implementTypes
            where !string.IsNullOrWhiteSpace(implementType.FullName)
            from method in implementType.GetMethods()
            let attrs = method.GetCustomAttributes(typeof(CronAttribute), false).As<IEnumerable<CronAttribute>>()
            from attr in attrs
            where attr.Enable
            let cron = CronExpression.Parse(attr.CronExpression, attr.Format)
            select new ScheduledInvokeData(cron, attr.Zone, attr.Inclusive, implementType.FullName!, method.Name);
        foreach (var jobInvokeData in jobInvokeDatas)
        {
            ScheduledData.Jobs.Add(Guid.NewGuid(), jobInvokeData);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ScheduledService>().StartAsync();
    }
}
