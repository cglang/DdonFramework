﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Cronos;
using Ddon.Schedule;
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

        var baseType = typeof(ISchedule);

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
                             select new ScheduleInvokeData(cron, attr.Zone, attr.Inclusive, implementType.FullName!, method.Name);
        foreach (var jobInvokeData in jobInvokeDatas)
        {
            ScheduleData.Jobs.Add(Guid.NewGuid(), jobInvokeData);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ScheduleService>().StartAsync(stoppingToken);
    }

    private static IEnumerable<ScheduleInvokeData> GetPathSchedule(string? path)
    {
        var result = new List<ScheduleInvokeData>();

        if (path is null) return result;

        string[] textFiles = Directory.GetFiles(path, "*.task");
        foreach (string filePath in textFiles)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                var item = ScheduleInvokeData.Parse(content);
                if (item is not null)
                    result.Add(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        return result;
    }
}
