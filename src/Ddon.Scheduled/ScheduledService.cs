using System;
using System.Threading.Tasks;
using Ddon.EventBus.Abstractions;
using Ddon.Scheduled.Event;
using Microsoft.Extensions.Logging;

namespace Ddon.Scheduled;

/// <summary>
/// Job 服务启动
/// </summary>
internal class ScheduledService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<ScheduledService> _logger;

    public ScheduledService(IEventBus eventBus, ILogger<ScheduledService> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        foreach (var job in ScheduledData.Jobs)
        {
            await ScheduledData.DelayQueue.AddAsync(job.Key, job.Value.NextSpan);
        }

        while (true)
        {
            var jobId = await ScheduledData.DelayQueue.TakeAsync();
            var job = ScheduledData.Jobs[jobId];

            var eventData = new ScheduledInvokeEventData(job.JobClassName, job.JobMethodName);
            try
            {
                await _eventBus!.PublishAsync(eventData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Scheduled 错误");
            }
            await ScheduledData.DelayQueue.AddAsync(jobId, job.NextSpan);
        }
    }
}
