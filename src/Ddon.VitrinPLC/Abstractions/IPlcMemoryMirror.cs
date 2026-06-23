using System;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    public interface IPlcMemoryMirror
    {
        long Version { get; }
        DateTime LastUpdateTime { get; }
        EndianFormat Endian { get; }
        byte[] GetRegion(string region);
        T Read<T>(TagDefinition tag);
    }
}
