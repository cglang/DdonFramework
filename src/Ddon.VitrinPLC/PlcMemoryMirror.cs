using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC
{
    /// <summary>
    /// 内存镜像实现。
    /// 只读对外暴露；唯一写入点是 SyncEngine 调用 ApplySnapshot()。
    /// </summary>
    public sealed class PlcMemoryMirror : IPlcMemoryMirror
    {
        private readonly ConcurrentDictionary<string, MemoryRegion> _regions = new();
        private long _version;

        public long Version => Interlocked.Read(ref _version);
        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;

        // ── 供 SyncEngine 注册区域 ────────────────────────
        public void RegisterRegion(string regionKey, string area, int startOffset, int length)
        {
            var region = new MemoryRegion(regionKey, area, startOffset, length);
            if (!_regions.TryAdd(regionKey, region))
                throw new InvalidOperationException($"Region '{regionKey}' 已注册。");
        }

        // ── 供 SyncEngine 整块替换 ────────────────────────
        /// <returns>旧 buffer（用于变化检测）</returns>
        public byte[] ApplySnapshot(string regionKey, byte[] newData)
        {
            if (!_regions.TryGetValue(regionKey, out var region))
                throw new KeyNotFoundException($"Region '{regionKey}' 未注册。");

            var old = region.Replace(newData);
            Interlocked.Increment(ref _version);
            LastUpdateTime = DateTime.UtcNow;
            return old;
        }

        // ── 对外只读接口 ──────────────────────────────────

        public byte[] GetRegion(string region)
        {
            if (!_regions.TryGetValue(region, out var r))
                throw new KeyNotFoundException($"Region '{region}' 未注册。");
            return r.GetSnapshot();
        }

        public T Read<T>(TagDefinition tag)
        {
            var addr = AddressParser.Parse(tag.Address, tag.Type);
            var snap = GetRegion(addr.RegionKey);
            return PlcCodec.Read<T>(snap, addr, tag.StringLength);
        }

        public IReadOnlyDictionary<string, MemoryRegionInfo> GetRegionInfo()
        {
            var result = new Dictionary<string, MemoryRegionInfo>();
            foreach (var kvp in _regions)
            {
                result[kvp.Key] = new MemoryRegionInfo
                {
                    RegionKey = kvp.Value.RegionKey,
                    Area = kvp.Value.Area,
                    StartOffset = kvp.Value.StartOffset,
                    Length = kvp.Value.Length
                };
            }
            return result;
        }
    }

    public sealed class MemoryRegionInfo
    {
        public string RegionKey { get; init; }
        public string Area { get; init; }
        public int StartOffset { get; init; }
        public int Length { get; init; }
    }
}
