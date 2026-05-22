using System;
using System.Collections.Generic;

namespace Ddon.VitrinPLC.Models
{
    // ─────────────────────────────────────────────
    // 扫描完成事件参数
    // ─────────────────────────────────────────────
    public sealed class ScanCompletedEventArgs : EventArgs
    {
        public long Version { get; init; }
        public DateTime CompletedAt { get; init; }
        public TimeSpan Elapsed { get; init; }
        public IReadOnlyList<TagChange> Changes { get; init; } = Array.Empty<TagChange>();
        public bool HasChanges => Changes.Count > 0;
    }
}
