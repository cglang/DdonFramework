using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ddon.Schedule;

/// <summary>
/// 计划服务启动
/// </summary>
internal class ScheduleService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IServiceProvider serviceProvider,
        IMediator mediator,
        ILogger<ScheduleService> logger)
    {
        _serviceProvider = serviceProvider;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        foreach (var job in ScheduleData.Schedules)
        {
            ScheduleData.DelayQueue.Add(job.Key, job.Value.NextSpan);
        }

        try
        {
            while (true)
            {
                var jobId = await ScheduleData.DelayQueue.TakeAsync(stoppingToken);

                await _mediator.Publish(new ScheduleInvokeEventData(jobId), stoppingToken);

                ScheduleData.DelayQueue.Add(jobId, ScheduleData.Schedules[jobId].NextSpan);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("计划服务延时队列已停止");
        }
        catch (Exception e)
        {
            throw new Exception("延时队列错误:队列停止", e);
        }
    }
}
