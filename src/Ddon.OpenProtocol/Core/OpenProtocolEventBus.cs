using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Core
{
    public sealed class OpenProtocolEventBus : IAsyncDisposable
    {
        private readonly Channel<Mid> _channel =
            Channel.CreateBounded<Mid>(new BoundedChannelOptions(1024)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
            });

        private readonly ConcurrentDictionary<int, List<Func<Mid, Task>>> _handlers = new();

        private readonly Task _dispatchTask;
        private readonly CancellationTokenSource _cts = new();

        public OpenProtocolEventBus()
        {
            _dispatchTask = Task.Run(DispatchLoop);
        }

        public IDisposable Subscribe<TMid>(Func<TMid, Task> handler) where TMid : Mid
        {
            int midNumber = ((TMid)Activator.CreateInstance(typeof(TMid))!).Header.Mid;
            return SubscribeByMid(midNumber, mid => handler((TMid)mid));
        }

        public IDisposable Subscribe<TMid>(Action<TMid> handler) where TMid : Mid
            => Subscribe<TMid>(mid =>
            {
                handler(mid);
                return Task.CompletedTask;
            });

        public IDisposable SubscribeAll(Func<Mid, Task> handler)
            => SubscribeByMid(-1, handler);

        private IDisposable SubscribeByMid(int midNumber, Func<Mid, Task> handler)
        {
            var list = _handlers.GetOrAdd(midNumber, _ => new List<Func<Mid, Task>>());

            lock (list) { list.Add(handler); }

            return new Subscription(() =>
            {
                lock (list) { list.Remove(handler); }
            });
        }

        public void Publish(Mid mid)
        {
            _channel.Writer.TryWrite(mid);
        }

        private async Task DispatchLoop()
        {
            await foreach (var mid in _channel.Reader.ReadAllAsync(_cts.Token))
            {
                await InvokeHandlers(mid.Header.Mid, mid);
                await InvokeHandlers(-1, mid);
            }
        }

        private async Task InvokeHandlers(int midNumber, Mid mid)
        {
            if (!_handlers.TryGetValue(midNumber, out var list))
                return;

            Func<Mid, Task>[] snapshot;

            lock (list) { snapshot = list.ToArray(); }

            foreach (var handler in snapshot)
            {
                try { await handler(mid); }
                catch { }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
            await _dispatchTask.ConfigureAwait(false);
            _cts.Dispose();
        }

        private sealed class Subscription(Action dispose) : IDisposable
        {
            public void Dispose() => dispose();
        }
    }
}
