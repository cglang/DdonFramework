using System;
using System.Collections.Generic;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    // ─────────────────────────────────────────────
    // 变化通知
    // ─────────────────────────────────────────────
    public interface IChangeNotifier
    {
        IDisposable Subscribe<T>(string tagName, Action<T> handler);
        void NotifyChanges(IEnumerable<TagChange> changes);
    }
}
