using System;

namespace Ddon.Core.Use.Queue
{
    public class DelayItem<T> : IDelayItem
    {
        /// <summary>
        /// 过期时间戳，绝对时间
        /// </summary>
        public readonly long TimeoutMilliseconds;

        public TimeSpan DelaySpan => TimeSpan.FromMilliseconds(Math.Max(TimeoutMilliseconds - GetTimestamp(), 0));

        /// <summary>
        /// 延时对象
        /// </summary>
        public readonly T Item;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeoutSpan">过期时间，相对时间</param>
        /// <param name="item">延时对象</param>
        public DelayItem(TimeSpan timeoutSpan, T item)
        {
            TimeoutMilliseconds = (long)timeoutSpan.TotalMilliseconds + GetTimestamp();
            Item = item;
        }

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

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        private long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        }
    }
}
