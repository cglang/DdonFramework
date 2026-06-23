using System;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    // ────────────────────────────────────────────────────────────
    // 调度策略接口
    // ────────────────────────────────────────────────────────────
    public interface IEventScheduler
    {
        void Schedule(Action action);

        Task ScheduleAsync(Func<Task> action);
    }
}
