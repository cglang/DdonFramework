using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Common.EventBus
{
    public sealed class GeneralEventBus
    {
        public static readonly GeneralEventBus Default = new GeneralEventBus();

        // 每种事件类型维护一份订阅列表（CopyOnWrite 语义）
        private readonly ConcurrentDictionary<Type, object> _channels = new ConcurrentDictionary<Type, object>();

        // ── 订阅 ──────────────────────────────────────────────
        /// <summary>
        /// 订阅事件 <typeparamref name="T"/>。
        /// <para>scheduler 为 null 时使用 <see cref="ImmediateScheduler"/>（在发布线程直接回调）。</para>
        /// </summary>
        public EventSubscription Subscribe<T>(
            Action<T> handler,
            IEventScheduler scheduler = null)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var channel = GetOrCreateChannel<T>();
            var entry = new SubscriberEntry<T>(handler, scheduler ?? ImmediateScheduler.Instance);
            channel.Add(entry);

            return new EventSubscription(() => channel.Remove(entry));
        }

        // ── 发布（同步）──────────────────────────────────────
        /// <summary>从任意线程发布事件，立即遍历所有订阅者并按其调度策略分发。</summary>
        public void Publish<T>(T evt)
        {
            var channel = TryGetChannel<T>();
            channel?.Dispatch(evt);
        }

        // ── 发布（异步等待所有回调完成）──────────────────────
        /// <summary>
        /// 发布事件并等待所有 <see cref="ThreadPoolScheduler"/> 上的回调执行完毕。
        /// ImmediateScheduler 的订阅者仍在当前线程同步执行。
        /// </summary>
        public Task PublishAsync<T>(T evt)
        {
            var channel = TryGetChannel<T>();
            return channel != null
                ? channel.DispatchAsync(evt)
                : Task.CompletedTask;
        }

        // ── 内部：Channel 管理 ────────────────────────────────
        private Channel<T> GetOrCreateChannel<T>() =>
            (Channel<T>)_channels.GetOrAdd(typeof(T), _ => new Channel<T>());

        private Channel<T> TryGetChannel<T>() =>
            _channels.TryGetValue(typeof(T), out var obj) ? (Channel<T>)obj : null;

        // ── 内部：Channel（单类型订阅列表）───────────────────
        private sealed class Channel<T>
        {
            // 使用 ReaderWriterLockSlim：读多写少（发布>>订阅/取消）
            private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            private readonly List<SubscriberEntry<T>> _entries = new List<SubscriberEntry<T>>();

            public void Add(SubscriberEntry<T> entry)
            {
                _lock.EnterWriteLock();
                try { _entries.Add(entry); }
                finally { _lock.ExitWriteLock(); }
            }

            public void Remove(SubscriberEntry<T> entry)
            {
                _lock.EnterWriteLock();
                try { _entries.Remove(entry); }
                finally { _lock.ExitWriteLock(); }
            }

            // 同步分发：按各自调度器分发，不等待异步完成
            public void Dispatch(T evt)
            {
                foreach (var entry in Snapshot())
                {
                    var captured = entry; // 避免闭包捕获变量
                    captured.Scheduler.Schedule(() =>
                    {
                        try { captured.Handler(evt); }
                        catch (Exception ex) { OnUnhandledException(ex, typeof(T)); }
                    });
                }
            }

            // 异步分发：等待所有回调完成
            public Task DispatchAsync(T evt)
            {
                var snapshot = Snapshot();
                if (snapshot.Count == 0) return Task.CompletedTask;

                var tasks = new List<Task>(snapshot.Count);
                foreach (var entry in snapshot)
                {
                    var captured = entry;
                    var tcs = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                    captured.Scheduler.Schedule(() =>
                    {
                        try
                        {
                            captured.Handler(evt);
                            tcs.TrySetResult(true);
                        }
                        catch (Exception ex)
                        {
                            OnUnhandledException(ex, typeof(T));
                            tcs.TrySetException(ex);
                        }
                    });

                    tasks.Add(tcs.Task);
                }

                return Task.WhenAll(tasks);
            }

            private List<SubscriberEntry<T>> Snapshot()
            {
                _lock.EnterReadLock();
                try { return new List<SubscriberEntry<T>>(_entries); }
                finally { _lock.ExitReadLock(); }
            }
        }

        // ── 异常处理 ──────────────────────────────────────────
        /// <summary>订阅者回调抛出异常时触发，默认输出到 Trace。可替换为自定义处理。</summary>
        public static Action<Exception, Type> UnhandledExceptionHandler { get; set; }
            = (ex, eventType) =>
                System.Diagnostics.Trace.TraceError(
                    $"[EventBus] 处理 {eventType.Name} 时发生异常: {ex}");

        private static void OnUnhandledException(Exception ex, Type eventType)
            => UnhandledExceptionHandler?.Invoke(ex, eventType);
    }
}
