using System;
using System.Threading;

namespace Ddon.VitrinPLC.Models
{

    // ─────────────────────────────────────────────
    // 内存区域块（供 Mirror 内部使用）
    // ─────────────────────────────────────────────
    public sealed class MemoryRegion
    {
        private byte[] _buffer;
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

        public string RegionKey { get; }
        public string Area { get; }
        public int StartOffset { get; }
        public int Length { get; }

        public MemoryRegion(string regionKey, string area, int startOffset, int length)
        {
            RegionKey = regionKey;
            Area = area;
            StartOffset = startOffset;
            Length = length;
            _buffer = new byte[length];
        }

        /// <summary>返回当前 buffer 的快照（只读安全副本）</summary>
        public byte[] GetSnapshot()
        {
            _lock.EnterReadLock();
            try
            {
                var copy = new byte[_buffer.Length];
                Buffer.BlockCopy(_buffer, 0, copy, 0, copy.Length);
                return copy;
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>原子替换整块 buffer（周期刷新调用）</summary>
        public byte[] Replace(byte[] newData)
        {
            if (newData.Length != _buffer.Length)
                throw new ArgumentException($"Region '{RegionKey}': 新数据长度 {newData.Length} ≠ 期望 {_buffer.Length}");

            _lock.EnterWriteLock();
            try
            {
                var old = _buffer;
                _buffer = newData;
                return old;
            }
            finally { _lock.ExitWriteLock(); }
        }
    }
}
