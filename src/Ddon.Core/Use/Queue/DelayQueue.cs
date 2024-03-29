﻿using System;
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

        public async Task<T?> TakeAsync(CancellationToken cancelToken = default)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (IsEmpty)
                { 
                    await Task.Delay(1, cancelToken);
                    continue;
                }

                var delayItem = _values.First();

                if (delayItem.DelaySpan <= TimeSpan.Zero)
                {
                    _values.Remove(delayItem);
                    return delayItem.Item;
                }

                await Task.Delay(1, cancelToken);
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
