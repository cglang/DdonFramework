using System;
using System.Diagnostics;
using System.Threading;

namespace Ddon.Core.Use.Queue
{
    public class DelayQueue<T> : SortedQueue<T> where T : class, IDelayItem
    {
        /// <summary>
        /// 当前排队等待取元素的线程
        /// </summary>
        private Thread? _waitThread = null;

        public bool TryTakeNoBlocking(out T? item)
        {
            lock (_lock)
            {
                item = Peek();
                if (item == null || item.DelaySpan > TimeSpan.Zero)
                {
                    item = null;
                    return false;
                }
                return Remove(item);
            }
        }

        public bool TryTake(out T? item, TimeSpan timeout, CancellationToken cancelToken)
        {
            item = null;

            if (DelayQueue<T>.IsTimeout(timeout, cancelToken))
            {
                throw new ArgumentException("Method execute timeout or cancelled");
            }

            if (!Monitor.TryEnter(_lock, timeout))
            {
                return false;
            }

            if (DelayQueue<T>.IsTimeout(timeout, cancelToken))
            {
                Monitor.Exit(_lock);
                return false;
            }

            try
            {
                while (!DelayQueue<T>.IsTimeout(timeout, cancelToken))
                {
                    // 当前没有项，阻塞等待
                    item = Peek();
                    if (item == null)
                    {
                        Monitor.Wait(_lock, timeout);
                        continue;
                    }

                    // 如果已经到期，则出队
                    var delaySpan = item.DelaySpan;
                    if (delaySpan <= TimeSpan.Zero)
                    {
                        return Remove(item);
                    }

                    // 移除引用，便于GC清理
                    item = null;

                    // 如果有其它线程也在等待，则阻塞等待
                    if (timeout < delaySpan || _waitThread != null)
                    {
                        Monitor.Wait(_lock, timeout);
                        //timeout = MonitorExtension.Wait(_lock, timeout);
                        continue;
                    }

                    // 否则当前线程设为等待线程
                    var thisThread = Thread.CurrentThread;
                    _waitThread = thisThread;

                    try
                    {
                        // 阻塞等待，如果有更早的项加入，会提前释放
                        // 否则等待delayMs时间，即当前项到期
                        // 注意，这里不能直接返回当前项，因为当前项可能被其它线程取出，所以要进入下一个循环获取
                        var timeLeft = Wait(_lock, delaySpan);
                        timeout -= delaySpan - timeLeft;
                        continue;
                    }
                    finally
                    {
                        // 释放出来，让其它线程也可以获取
                        if (_waitThread == thisThread)
                        {
                            _waitThread = null;
                        }
                    }
                }

                return false;
            }
            finally
            {
                // 当前线程已取到项，且还有剩余项，则唤醒其它就绪的线程
                if (_waitThread == null && IsEmpty)
                {
                    Monitor.Pulse(_lock);
                }

                Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// 是否超时
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        private static bool IsTimeout(TimeSpan timeout, CancellationToken cancelToken)
        {
            return timeout <= TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan || cancelToken.IsCancellationRequested;
        }

        /// <summary>
        /// 锁等待，返回剩余时间
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="timeout">超时时间，如果是Infinite则无限期等待</param>
        /// <returns></returns>
        private static TimeSpan Wait(object obj, TimeSpan timeout)
        {
            // Monitor.Wait阻塞并释放锁，将当前线程置于等待队列，直至Monitor.Pulse通知其进入就绪队列，
            // 或者超时未接到通知，自动进入就绪队列。
            // timeout是进入就绪队列之前等待的时间，返回false表示已超时。
            // 进入就绪队列后会尝试获取锁，但直至拿到锁之前都不会返回值。

            var sw = Stopwatch.StartNew();
            Monitor.Wait(obj, timeout);
            sw.Stop();

            return timeout - sw.Elapsed;
        }
    }
}
