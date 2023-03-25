using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Queue
{
    public class DelayQueue<T> where T : IEquatable<T>
    {
        private readonly SortedQueue<DelayItem<T>> _values = new();

        public int Count => _values.Count;

        public bool IsEmpty => _values.IsEmpty;

        public async Task<T?> TakeAsync(CancellationToken? cancelToken = null)
        {
            cancelToken ??= CancellationToken.None;

            while (!cancelToken.Value.IsCancellationRequested)
            {
                if (!_values.Any())
                {
                    await Task.Delay(1);
                    return default;
                }

                var delayItem = _values.First();

                if (delayItem.DelaySpan <= TimeSpan.Zero)
                {
                    _values.Remove(delayItem);
                    return delayItem.Item;
                }

                await Task.Delay(1);
            }

            return default;
        }

        public bool Add(T item, TimeSpan time)
        {
            try
            {
                _values.Add(new DelayItem<T>(time, item));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Remove(T item)
        {
            var data = GetItem(item);
            if (data is null) throw new Exception("队列中不存在该项[移除失败]");
            _values.Remove(data);
        }

        public DelayItem<T>? GetItem(T item)
        {
            return _values.FirstOrDefault(x => x.Item.Equals(item));
        }

        public void Clear()
        {
            _values.Clear();
        }
    }
}
