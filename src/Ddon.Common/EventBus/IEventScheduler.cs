using System;

namespace Ddon.Common.EventBus
{
    // ────────────────────────────────────────────────────────────
    // 调度策略接口
    // ────────────────────────────────────────────────────────────
    public interface IEventScheduler
    {
        void Schedule(Action action);
    }
}
