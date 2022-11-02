using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Core.Use.Queue
{
    public class SortedQueue<T> : SortedSet<T> where T : class, IDelayItem
    {
        protected internal readonly object _lock = new();

        public new int Count
        {
            get
            {
                lock (_lock)
                {
                    return Count;
                }
            }
        }

        public bool IsEmpty => Count == 0;

        public SortedQueue() : base(new SortComparer()) { }

        public T? Peek()
        {
            lock (_lock)
            {
                return this.FirstOrDefault();
            }
        }

        private sealed class SortComparer : Comparer<T>
        {
            public override int Compare(T? x, T? y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                // 先用默认比较器比较，如果相等，再用HashCode比较
                var result = Default.Compare(x, y);
                if (result == 0)
                {
                    // 如果HashCode相等，表示同一个对象，不允许重复添加
                    result = x.GetHashCode().CompareTo(y.GetHashCode());
                }

                return result;
            }
        }
    }
}
