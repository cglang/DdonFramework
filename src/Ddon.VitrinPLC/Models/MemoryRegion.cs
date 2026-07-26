using System;
using System.Threading;

namespace Ddon.VitrinPLC.Models;

public sealed class MemoryRegion
{
    private BufferSlice _data;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    public string RegionKey { get; }
    public string Area { get; }

    public BufferSlice Data => _data;
    public int Length => _data.Length;

    public MemoryRegion(string regionKey, string area, BufferSlice initial)
    {
        RegionKey = regionKey;
        Area = area;
        _data = initial;
    }

    public byte[] GetSnapshot()
    {
        _lock.EnterReadLock();
        try
        {
            return _data.Snapshot();
        }
        finally { _lock.ExitReadLock(); }
    }

    public BufferSlice Replace(BufferSlice newData)
    {
        _lock.EnterWriteLock();
        try
        {
            var old = _data;
            _data = newData;
            return old;
        }
        finally { _lock.ExitWriteLock(); }
    }
}
