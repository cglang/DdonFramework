using System;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    /// <summary>
    /// 直接在发布线程执行（后台订阅者默认）
    /// </summary>
    public sealed class ImmediateScheduler : IEventScheduler
    {
        public static readonly ImmediateScheduler Instance = new ImmediateScheduler();
        public void Schedule(Action action) => action();
        public Task ScheduleAsync(Func<Task> action) => action();
    }
}
