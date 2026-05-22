using System;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    // ─────────────────────────────────────────────
    // 内存镜像层：只读缓存
    // ─────────────────────────────────────────────
    public interface IPlcMemoryMirror
    {
        long Version { get; }
        DateTime LastUpdateTime { get; }
        byte[] GetRegion(string region);
        T Read<T>(TagDefinition tag);
    }
}
