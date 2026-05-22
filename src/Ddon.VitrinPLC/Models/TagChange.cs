using System;

namespace Ddon.VitrinPLC.Models
{
    // ─────────────────────────────────────────────
    // 变化条目
    // ─────────────────────────────────────────────
    public sealed class TagChange
    {
        public TagDefinition Tag { get; init; }
        public object OldValue { get; init; }
        public object NewValue { get; init; }
        public DateTime ChangedAt { get; init; } = DateTime.UtcNow;

        public override string ToString() =>
            $"{Tag.Name}: {OldValue} → {NewValue}  ({ChangedAt:HH:mm:ss.fff})";
    }
}
