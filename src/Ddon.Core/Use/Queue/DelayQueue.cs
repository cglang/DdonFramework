using System;
using System.Linq;
using System.Threading;

namespace Ddon.Core.Use.Queue
{
    public class DelayQueue<T> : SortedQueue<T> where T : class, IDelayItem
    {
        private readonly object _lock = new();

        public bool TryTakeNoBlocking(out T? item)
        {
            lock (_lock)
            {
                item = this.FirstOrDefault();
                if (item == null || item.DelaySpan > TimeSpan.Zero)
                {
                    item = null;
                    return false;
                }
                return Remove(item);
            }
        }

        /// <summary>
        /// 取出项，如果未到期，则阻塞
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public T? Take(CancellationToken? cancelToken = null)
        {
            if (cancelToken == null) 
                cancelToken = CancellationToken.None;

            T? item = null;
            Monitor.Enter(_lock);

            try
            {
                while (!cancelToken.Value.IsCancellationRequested)
                {
                    if (!this.Any())
                    {
                        Monitor.Pulse(_lock);
                        Monitor.Wait(_lock);
                        continue;
                    }

                    item = this.First();

                    if (item.DelaySpan <= TimeSpan.Zero)
                    {
                        Remove(item);
                        return item;
                    }
                }

                return item;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }
}
