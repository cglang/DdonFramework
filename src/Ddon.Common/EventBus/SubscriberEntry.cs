using System;

namespace Ddon.Common.EventBus
{
    // ────────────────────────────────────────────────────────────
    // 内部：订阅条目
    // ────────────────────────────────────────────────────────────
    internal sealed class SubscriberEntry<T>
    {
        public readonly Action<T> Handler;
        public readonly IEventScheduler Scheduler;

        public SubscriberEntry(Action<T> handler, IEventScheduler scheduler)
        {
            Handler = handler;
            Scheduler = scheduler;
        }
    }
}
