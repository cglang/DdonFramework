using System;
using System.Threading;

namespace Ddon.Common.EventBus
{
    // ────────────────────────────────────────────────────────────
    // 订阅句柄（Dispose 即取消订阅）
    // ────────────────────────────────────────────────────────────
    public sealed class EventSubscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private int _disposed;

        internal EventSubscription(Action unsubscribe) => _unsubscribe = unsubscribe;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                _unsubscribe();
        }
    }
}
