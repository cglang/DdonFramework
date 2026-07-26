using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC
{
    public sealed class PlcMemoryMirror : IPlcMemoryMirror
    {
        private readonly IPlcAddressParser _parser;
        private readonly ConcurrentDictionary<string, MemoryRegion> _regions = new();
        private long _version;

        public long Version => Interlocked.Read(ref _version);
        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;
        public EndianFormat Endian { get; }

        public PlcMemoryMirror(EndianFormat endian, IPlcAddressParser parser)
        {
            Endian = endian;
            _parser = parser;
        }

        public void RegisterRegion(string regionKey, string area)
        {
            var region = new MemoryRegion(regionKey, area, new BufferSlice([], 0));
            if (!_regions.TryAdd(regionKey, region))
                throw new InvalidOperationException($"Region '{regionKey}' 已注册。");
        }

        public BufferSlice ApplySnapshot(string regionKey, BufferSlice newData)
        {
            if (!_regions.TryGetValue(regionKey, out var region))
                throw new KeyNotFoundException($"Region '{regionKey}' 未注册。");

            var old = region.Replace(newData);
            Interlocked.Increment(ref _version);
            LastUpdateTime = DateTime.UtcNow;
            return old;
        }

        public BufferSlice GetRegion(string region)
        {
            if (!_regions.TryGetValue(region, out var r))
                throw new KeyNotFoundException($"Region '{region}' 未注册。");
            return r.Data;
        }

        public T Read<T>(TagDefinition tag)
        {
            var addr = _parser.Parse(tag.Address, tag.Type);
            var data = GetRegion(addr.RegionKey);
            return PlcCodec.Read<T>(data, addr, tag.StringLength, Endian);
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
                    Length = kvp.Value.Length
                };
            }
            return result;
        }
    }
}
