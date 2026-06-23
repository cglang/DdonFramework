using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    /// <summary>
    /// 投递到指定 TaskScheduler（如 TaskScheduler.FromCurrentSynchronizationContext()）
    /// </summary>
    public sealed class TaskSchedulerScheduler : IEventScheduler
    {
        private readonly TaskScheduler _scheduler;

        public TaskSchedulerScheduler(TaskScheduler scheduler)
            => _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

        public void Schedule(Action action) 
            => Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, _scheduler);

        public Task ScheduleAsync(Func<Task> action)
            => Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, _scheduler)
               .Unwrap();
    }
}
