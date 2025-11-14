using System;
using System.Threading.Tasks;

namespace Ddon.Common.Utilities.WheelTimer
{
    public sealed partial class HashedWheelTimer
    {        
        private sealed class HashedWheelTimeout : ITimeout
        {
            public Func<Task> CallbackAsync; // 异步回调
            public Action Callback;          // 同步回调

            public readonly TimeSpan Delay;

            public long RemainingRounds;

            public HashedWheelTimeout Prev;
            public HashedWheelTimeout Next;

            public bool Cancelled;

            public HashedWheelTimeout(Action callback, TimeSpan delay)
            {
                Callback = callback;
                Delay = delay;
            }

            public HashedWheelTimeout(Func<Task> callbackAsync, TimeSpan delay)
            {
                CallbackAsync = callbackAsync;
                Delay = delay;
            }

            public void Cancel() => Cancelled = true;
        }
    }

}
