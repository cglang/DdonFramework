using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Queue
{
    public class DelayQueue<T> where T : IEquatable<T>
    {
        private TaskCompletionSource _queueWaitIsEmpty;

        private readonly SortedQueue<DelayItem<T>> _values = new();

        public DelayQueue()
        {
            _queueWaitIsEmpty = new TaskCompletionSource();
        }

        public int Count => _values.Count;

        public bool IsEmpty => _values.IsEmpty;

        public async Task<T?> TakeAsync(CancellationToken cancelToken = default)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (!_values.IsEmpty)
                {
                    _queueWaitIsEmpty = new TaskCompletionSource(cancelToken);
                    //_queueWaitIsEmpty.SetCanceled(cancelToken);
                    await _queueWaitIsEmpty.Task;
                    continue;
                }

                var delayItem = _values.First();

                if (delayItem.DelaySpan <= TimeSpan.Zero)
                {
                    _values.Remove(delayItem);
                    return delayItem.Item;
                }

                await Task.Delay(100, cancelToken);
            }

            return default;
        }

        public bool Add(T item, TimeSpan time)
        {
            try
            {
                _values.Add(new DelayItem<T>(time, item));
                _queueWaitIsEmpty.SetResult();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Remove(T item)
        {
            foreach (var delayItem in _values.Where(x => x.Item.Equals(item)))
            {
                _values.Remove(delayItem);
            }
        }

        public void Clear()
        {
            _values.Clear();
        }
    }
}
