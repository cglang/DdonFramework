using System;
using System.Threading.Tasks;
using Ddon.EventBus.Abstractions;
using Ddon.Job.Event;
using Microsoft.Extensions.Logging;

namespace Ddon.Job
{
    /// <summary>
    /// Job 服务启动
    /// </summary>
    internal class JobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventBus _eventBus;
        private readonly ILogger<JobService> _logger;

        public JobService(IServiceProvider serviceProvider, IEventBus eventBus, ILogger<JobService> logger)
        {
            _serviceProvider = serviceProvider;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            foreach (var job in JobData.Jobs)
            {
                await JobData.DelayQueue.AddAsync(job.Key, job.Value.NextSpan);
            }

            while (true)
            {
                var jobId = await JobData.DelayQueue.TakeAsync();
                var job = JobData.Jobs[jobId];

                var eventData = new JobInvokeEventData(job.JobClassName, job.JobMethodName);
                try
                {
                    await _eventBus!.PublishAsync(eventData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Job错误");
                }
                await JobData.DelayQueue.AddAsync(jobId, job.NextSpan);
            }
        }
    }
}
