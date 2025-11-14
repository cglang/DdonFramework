namespace Ddon.Common.Utilities.WheelTimer
{
    public sealed partial class HashedWheelTimer
    {
        private sealed class HashedWheelBucket
        {
            private HashedWheelTimeout head;
            private HashedWheelTimeout tail;

            private readonly ITimeoutExecutor _executor;

            public HashedWheelBucket(ITimeoutExecutor executor)
            {
                _executor = executor;
            }

            public void Add(HashedWheelTimeout timeout)
            {
                if (head == null)
                {
                    head = tail = timeout;
                    return;
                }
                tail.Next = timeout;
                timeout.Prev = tail;
                tail = timeout;
            }

            public void ExpireTimeouts()
            {
                var cur = head;
                while (cur != null)
                {
                    var next = cur.Next;

                    if (cur.Cancelled)
                    {
                        Remove(cur);
                    }
                    else if (cur.RemainingRounds <= 0)
                    {
                        if (cur.CallbackAsync != null)
                            _executor.Execute(cur.CallbackAsync);
                        else if (cur.Callback != null)
                            _executor.Execute(cur.Callback);

                        Remove(cur);
                    }
                    else
                    {
                        cur.RemainingRounds--;
                    }

                    cur = next;
                }
            }

            private void Remove(HashedWheelTimeout timeout)
            {
                var next = timeout.Next;
                var prev = timeout.Prev;

                if (prev != null)
                    prev.Next = next;
                if (next != null)
                    next.Prev = prev;

                if (timeout == head) head = next;
                if (timeout == tail) tail = prev;

                timeout.Prev = timeout.Next = null;
            }
        }
    }
}
