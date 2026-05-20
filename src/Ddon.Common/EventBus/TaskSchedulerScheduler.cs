using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    // 投递到指定 TaskScheduler（如 TaskScheduler.FromCurrentSynchronizationContext()）
    public sealed class TaskSchedulerScheduler : IEventScheduler
    {
        private readonly TaskScheduler _scheduler;

        public TaskSchedulerScheduler(TaskScheduler scheduler)
            => _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

        public void Schedule(Action action) =>
            Task.Factory.StartNew(action,
                CancellationToken.None,
                TaskCreationOptions.None,
                _scheduler);
    }
}
