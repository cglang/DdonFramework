using System;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    internal sealed class SubscriberEntry<T>
    {
        public readonly Func<T, Task> Handler;
        public readonly IEventScheduler Scheduler;

        // 异步 handler 直接存
        public SubscriberEntry(Func<T, Task> handler, IEventScheduler scheduler)
        {
            Handler = handler;
            Scheduler = scheduler;
        }

        // 同步 handler 包装成 Func<T, Task>
        public SubscriberEntry(Action<T> handler, IEventScheduler scheduler)
        {
            Handler = e => { handler(e); return Task.CompletedTask; };
            Scheduler = scheduler;
        }
    }
}
