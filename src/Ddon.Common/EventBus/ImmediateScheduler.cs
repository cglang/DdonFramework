using System;

namespace Ddon.Common.EventBus
{
    // 直接在发布线程执行（后台订阅者默认）
    public sealed class ImmediateScheduler : IEventScheduler
    {
        public static readonly ImmediateScheduler Instance = new ImmediateScheduler();
        public void Schedule(Action action) => action();
    }
}
