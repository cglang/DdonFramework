using System;
using System.Threading;

namespace Ddon.Common.EventBus
{
    // 投递到线程池
    public sealed class ThreadPoolScheduler : IEventScheduler
    {
        public static readonly ThreadPoolScheduler Instance = new ThreadPoolScheduler();
        public void Schedule(Action action) =>
            ThreadPool.QueueUserWorkItem(_ => action());
    }
}
