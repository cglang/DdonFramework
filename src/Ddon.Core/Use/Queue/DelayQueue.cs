using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Queue
{
    public class DelayQueue<T>
    {
        private readonly SortedQueue<DelayItem<T>> values = new();

        private static readonly object _lock = new();

        public int Count => values.Count;

        public bool IsEmpty => values.IsEmpty;

        public T? Take(CancellationToken? cancelToken = null)
        {
            if (cancelToken == null)
                cancelToken = CancellationToken.None;
            Monitor.Enter(_lock);

            try
            {
                while (!cancelToken.Value.IsCancellationRequested)
                {
                    if (!values.Any()) return default;

                    var delayItem = values.First();

                    if (delayItem.DelaySpan <= TimeSpan.Zero)
                    {
                        values.Remove(delayItem);
                        return delayItem.Item;
                    }

                    Monitor.Pulse(_lock);
                    Monitor.Wait(_lock);
                    Thread.Sleep(1);
                }

                return default;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public async Task<T?> TakeAsync(CancellationToken? cancelToken = null)
        {
            if (cancelToken == null)
                cancelToken = CancellationToken.None;

            while (!cancelToken.Value.IsCancellationRequested)
            {
                if (!values.Any()) return default;

                var delayItem = values.First();

                if (delayItem.DelaySpan <= TimeSpan.Zero)
                {
                    values.Remove(delayItem);
                    return delayItem.Item;
                }

                await Task.Delay(1);
            }
            return default;
        }

        public async Task<bool> AddAsync(T item, TimeSpan time)
        {
            try
            {
                await Task.Run(() => values.Add(new(time, item)));
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
