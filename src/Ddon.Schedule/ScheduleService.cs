using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ddon.Schedule;

/// <summary>
/// Job 服务启动
/// </summary>
internal class ScheduleService
{
    private readonly IMediator _mediator;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(IMediator mediator, ILogger<ScheduleService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        foreach (var job in ScheduleData.Jobs)
        {
            ScheduleData.DelayQueue.Add(job.Key, job.Value.NextSpan);
        }

        try
        {
            while (true)
            {
                var jobId = await ScheduleData.DelayQueue.TakeAsync();
                var job = ScheduleData.Jobs[jobId];

                var eventData = new ScheduleInvokeEventData(job.JobClassName, job.JobMethodName);
                try
                {
                    await _mediator.Publish(eventData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Schedule 错误");
                }

                ScheduleData.DelayQueue.Add(jobId, job.NextSpan);
            }
        }
        catch (Exception e)
        {
            throw new Exception("延时队列错误:队列停止", e);
        }
    }
}
