using System;
using System.Threading.Tasks;
using Ddon.EventBus.Abstractions;
using Ddon.Job.Event;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Job.old
{
    /// <summary>
    /// Job 服务启动
    /// </summary>
    internal class JobService
    {
        private readonly IEventBus _eventBus;

        public JobService(IServiceProvider serviceProvider)
        {
            _eventBus = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IEventBus>();
        }

        public void Start()
        {
            Task.Run(async () =>
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
                    await _eventBus.PublishAsync(eventData);

                    await JobData.DelayQueue.AddAsync(jobId, job.NextSpan);
                }
            });
        }
    }
}
