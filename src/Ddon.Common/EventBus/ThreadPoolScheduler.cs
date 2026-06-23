using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    /// <summary>
    /// 投递到线程池
    /// </summary>
    public sealed class ThreadPoolScheduler : IEventScheduler
    {
        public static readonly ThreadPoolScheduler Instance = new ThreadPoolScheduler();
        public void Schedule(Action action) => ThreadPool.QueueUserWorkItem(_ => action());

        public Task ScheduleAsync(Func<Task> action) => Task.Run(action);
    }
}
