using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.TagEngine
{
    /// <summary>
    /// 基于旧镜像 vs 新镜像差异发布变化通知。
    /// 订阅者通过 Dispose 自动移除，无内存泄漏。
    /// </summary>
    public sealed class ChangeNotifier : IChangeNotifier
    {
        // tagName → List of Subscription
        private readonly ConcurrentDictionary<string, List<Subscription>> _subs =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly object _listLock = new();

        public IDisposable Subscribe<T>(string tagName, Action<T> handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            var sub = new Subscription(tagName, v =>
            {
                try { handler((T)Convert.ChangeType(v, typeof(T))); }
                catch { /* 忽略类型转换错误，防止通知链断裂 */ }
            }, this);

            lock (_listLock)
            {
                if (!_subs.TryGetValue(tagName, out var list))
                {
                    list = new List<Subscription>();
                    _subs[tagName] = list;
                }
                list.Add(sub);
            }

            return sub;
        }

        public void NotifyChanges(IEnumerable<TagChange> changes)
        {
            foreach (var change in changes)
            {
                if (!_subs.TryGetValue(change.Tag.Name, out var list)) continue;

                List<Subscription> toRemove = null;
                lock (_listLock)
                {
                    foreach (var sub in list)
                    {
                        if (sub.IsAlive)
                            sub.Invoke(change.NewValue);
                        else
                            (toRemove ??= new()).Add(sub);
                    }
                    if (toRemove != null)
                        foreach (var dead in toRemove) list.Remove(dead);
                }
            }
        }

        public void Unsubscribe(string tagName, Subscription sub)
        {
            lock (_listLock)
            {
                if (_subs.TryGetValue(tagName, out var list))
                    list.Remove(sub);
            }
        }

        // ── 内部订阅句柄 ──────────────────────────────────
        public sealed class Subscription : IDisposable
        {
            private readonly string _tagName;
            private readonly Action<object> _handler;
            private readonly ChangeNotifier _owner;
            private volatile bool _disposed;

            public bool IsAlive => !_disposed;

            public Subscription(string tagName, Action<object> handler, ChangeNotifier owner)
            {
                _tagName = tagName;
                _handler = handler;
                _owner = owner;
            }

            public void Invoke(object value)
            {
                if (!_disposed) _handler(value);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _owner.Unsubscribe(_tagName, this);
            }
        }
    }
}
