using System;
using System.Collections.Generic;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    public interface IPlcMemoryMirror
    {
        long Version { get; }
        DateTime LastUpdateTime { get; }
        EndianFormat Endian { get; }
        BufferSlice GetRegion(string region);
        T Read<T>(TagDefinition tag);
        BufferSlice ApplySnapshot(string regionKey, BufferSlice newData);
        IReadOnlyDictionary<string, MemoryRegionInfo> GetRegionInfo();
        void RegisterRegion(string regionKey, string area);
    }
}
