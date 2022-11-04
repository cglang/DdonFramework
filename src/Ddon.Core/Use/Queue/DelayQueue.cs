﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public T? Take(CancellationToken? cancelToken = null)
        {
            if (cancelToken == null)
                cancelToken = CancellationToken.None;
            Monitor.Enter(_lock);

            try
            {
                while (!cancelToken.Value.IsCancellationRequested)
                {
                    if (!this.Any()) return null;

                    T? item = this.First();

                    if (item.DelaySpan <= TimeSpan.Zero)
                    {
                        Remove(item);
                        return item;
                    }

                    Monitor.Pulse(_lock);
                    Monitor.Wait(_lock);
                    Thread.Sleep(1);
                }

                return null;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public bool TryTake(out T? item)
        {
            item = Take();
            return item != null;
        }
    }
}
