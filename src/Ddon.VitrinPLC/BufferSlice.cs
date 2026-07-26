using System;

namespace Ddon.VitrinPLC;

public sealed class BufferSlice
{
    private readonly byte[] _data;
    private readonly int _offset;

    public int Length => _data.Length;
    public int Offset => _offset;

    public BufferSlice(byte[] data, int offset = 0)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _offset = offset;
    }

    public byte ReadByte(int index)
    {
        var i = index - _offset;
        return (uint)i < (uint)_data.Length ? _data[i] : (byte)0;
    }

    /// <summary>读取指定范围，越界部分补 0</summary>
    public byte[] ReadBytes(int index, int count)
    {
        var result = new byte[count];
        var srcStart = index - _offset;
        var srcLen = Math.Max(0, Math.Min(count, _data.Length - Math.Max(0, srcStart)));
        if (srcLen > 0)
            Buffer.BlockCopy(_data, Math.Max(0, srcStart), result, 0, srcLen);
        return result;
    }

    public byte[] Snapshot() => (byte[])_data.Clone();
}
