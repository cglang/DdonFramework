using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Common.Utilities.WheelTimer;

namespace Ddon.Common.Utilities
{
    public class DelayQueue<T>
    {
        private readonly HashedWheelTimer _timer;
        private readonly ConcurrentQueue<T> _readyQueue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public DelayQueue()
        {
            _timer = new HashedWheelTimer(TimeSpan.FromMilliseconds(10));
        }

        public void Enqueue(T item, TimeSpan delay)
        {
            _timer.NewTimeout(() =>
            {
                _readyQueue.Enqueue(item);
                _signal.Release(); // 通知消费者
            }, delay);
        }

        public T Dequeue(CancellationToken cancellationToken = default)
        {
            _signal.Wait(cancellationToken);
            _readyQueue.TryDequeue(out var item);
            return item;
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            await _signal.WaitAsync(cancellationToken);
            _readyQueue.TryDequeue(out var item);
            return item;
        }

        public int Count => _readyQueue.Count;
    }
}
