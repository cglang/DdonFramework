using System;
using System.Threading;

namespace Ddon.Core.Use.Queue
{
    public class DelayItem<T> : IDelayItem
    {
        /// <summary>
        /// 过期时间戳，绝对时间
        /// </summary>
        public readonly long TimeoutMilliseconds;

        public TimeSpan DelaySpan { get; private set; }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is DelayItem<T> value)
            {
                return TimeoutMilliseconds.CompareTo(value.TimeoutMilliseconds);
            }

            throw new ArgumentException($"Object is not a {nameof(DelayItem<T>)}");
        }
    }
}
