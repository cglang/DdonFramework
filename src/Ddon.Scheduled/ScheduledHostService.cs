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
    public IServiceProvider _serviceProvider { get; }

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

        foreach (var implementType in implementTypes)
        {
            if (string.IsNullOrWhiteSpace(implementType.FullName)) continue;

            foreach (var method in implementType.GetMethods())
            {
                var attrs = method.GetCustomAttributes(typeof(CronAttribute), false).As<IEnumerable<CronAttribute>>();
                foreach (var attr in attrs)
                {
                    if (attr.Enable == false) continue;

                    var cron = CronExpression.Parse(attr.CronExpression, attr.Format);
                    var jobInvokeData = new ScheduledInvokeData(cron, attr.Zone, attr.Inclusive, implementType.FullName, method.Name);
                    ScheduledData.Jobs.Add(Guid.NewGuid(), jobInvokeData);
                }
            }
        }

        using var scope = _serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ScheduledService>().StartAsync();
    }
}
