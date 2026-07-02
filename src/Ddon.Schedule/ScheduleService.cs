using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ddon.Schedule;

/// <summary>
/// 计划服务启动
/// </summary>
internal class ScheduleService
{
    private readonly ScheduleInvokeHandler _handler;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        ScheduleInvokeHandler handler,
        ILogger<ScheduleService> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        foreach (var job in ScheduleData.Schedules)
        {
            ScheduleData.DelayQueue.Enqueue(job.Key, job.Value.NextSpan);
        }

        try
        {
            while (true)
            {
                var jobId = await ScheduleData.DelayQueue.TakeAsync(stoppingToken);

                await _handler.Handle(new ScheduleInvokeEventData(jobId), stoppingToken);

                ScheduleData.DelayQueue.Enqueue(jobId, ScheduleData.Schedules[jobId].NextSpan);
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
