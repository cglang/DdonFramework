using System;
using System.Text;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC
{
    /// <summary>
    /// 在 byte[] 与强类型之间做编解码。
    /// PLC 通常使用 Big-Endian（西门子、三菱），可按需扩展 Little-Endian。
    /// </summary>
    public static class PlcCodec
    {
        // ─────────────────────────────────────────────
        // 从 buffer 读值
        // ─────────────────────────────────────────────
        public static T Read<T>(byte[] buffer, ParsedAddress addr, int stringLength = 0)
        {
            object value = addr.DataType switch
            {
                PlcDataType.Bool => ReadBool(buffer, addr.ByteOffset, addr.BitIndex),
                PlcDataType.Byte => buffer[addr.ByteOffset],
                PlcDataType.Int16 => ReadInt16(buffer, addr.ByteOffset),
                PlcDataType.UInt16 => ReadUInt16(buffer, addr.ByteOffset),
                PlcDataType.Int32 => ReadInt32(buffer, addr.ByteOffset),
                PlcDataType.UInt32 => ReadUInt32(buffer, addr.ByteOffset),
                PlcDataType.Float => ReadFloat(buffer, addr.ByteOffset),
                PlcDataType.Double => ReadDouble(buffer, addr.ByteOffset),
                PlcDataType.String => ReadString(buffer, addr.ByteOffset, stringLength > 0 ? stringLength : 256),
                _ => throw new NotSupportedException($"不支持的类型: {addr.DataType}")
            };

            return (T)Convert.ChangeType(value, typeof(T));
        }

        // ─────────────────────────────────────────────
        // 将值编码为 byte[]（用于写入 PLC）
        // ─────────────────────────────────────────────
        public static byte[] Encode<T>(T value, PlcDataType type, int byteOffset = 0, int bitIndex = 0, int stringLength = 0)
        {
            return type switch
            {
                PlcDataType.Bool => EncodeBool(Convert.ToBoolean(value), byteOffset, bitIndex),
                PlcDataType.Byte => new[] { Convert.ToByte(value) },
                PlcDataType.Int16 => EncodeInt16(Convert.ToInt16(value)),
                PlcDataType.UInt16 => EncodeUInt16(Convert.ToUInt16(value)),
                PlcDataType.Int32 => EncodeInt32(Convert.ToInt32(value)),
                PlcDataType.UInt32 => EncodeUInt32(Convert.ToUInt32(value)),
                PlcDataType.Float => EncodeFloat(Convert.ToSingle(value)),
                PlcDataType.Double => EncodeDouble(Convert.ToDouble(value)),
                PlcDataType.String => EncodeString(Convert.ToString(value), stringLength > 0 ? stringLength : 256),
                _ => throw new NotSupportedException($"不支持的类型: {type}")
            };
        }

        // ─────────────────────────────────────────────
        // Bool（位操作）
        // ─────────────────────────────────────────────
        private static bool ReadBool(byte[] buf, int byteOff, int bit)
        {
            GuardBounds(buf, byteOff, 1);
            return (buf[byteOff] & (1 << bit)) != 0;
        }

        private static byte[] EncodeBool(bool val, int byteOff, int bit)
        {
            // 返回单字节，由调用方按位合并
            byte b = val ? (byte)(1 << bit) : (byte)0;
            return new[] { b };
        }

        // ─────────────────────────────────────────────
        // 整数（Big-Endian）
        // ─────────────────────────────────────────────
        private static short ReadInt16(byte[] buf, int off) { GuardBounds(buf, off, 2); return (short)((buf[off] << 8) | buf[off + 1]); }
        private static ushort ReadUInt16(byte[] buf, int off) { GuardBounds(buf, off, 2); return (ushort)((buf[off] << 8) | buf[off + 1]); }
        private static int ReadInt32(byte[] buf, int off) { GuardBounds(buf, off, 4); return (buf[off] << 24) | (buf[off + 1] << 16) | (buf[off + 2] << 8) | buf[off + 3]; }
        private static uint ReadUInt32(byte[] buf, int off) { GuardBounds(buf, off, 4); return (uint)ReadInt32(buf, off); }

        private static byte[] EncodeInt16(short v) => new[] { (byte)(v >> 8), (byte)v };
        private static byte[] EncodeUInt16(ushort v) => new[] { (byte)(v >> 8), (byte)v };
        private static byte[] EncodeInt32(int v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };
        private static byte[] EncodeUInt32(uint v) => EncodeInt32((int)v);

        // ─────────────────────────────────────────────
        // 浮点
        // ─────────────────────────────────────────────
        private static float ReadFloat(byte[] buf, int off)
        {
            GuardBounds(buf, off, 4);
            var tmp = new byte[4];
            Buffer.BlockCopy(buf, off, tmp, 0, 4);
            if (BitConverter.IsLittleEndian) Array.Reverse(tmp);
            return BitConverter.ToSingle(tmp, 0);
        }

        private static double ReadDouble(byte[] buf, int off)
        {
            GuardBounds(buf, off, 8);
            var tmp = new byte[8];
            Buffer.BlockCopy(buf, off, tmp, 0, 8);
            if (BitConverter.IsLittleEndian) Array.Reverse(tmp);
            return BitConverter.ToDouble(tmp, 0);
        }

        private static byte[] EncodeFloat(float v)
        {
            var b = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        private static byte[] EncodeDouble(double v)
        {
            var b = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        // ─────────────────────────────────────────────
        // 字符串（ASCII，定长）
        // ─────────────────────────────────────────────
        private static string ReadString(byte[] buf, int off, int len)
        {
            GuardBounds(buf, off, len);
            return Encoding.ASCII.GetString(buf, off, len).TrimEnd('\0');
        }

        private static byte[] EncodeString(string s, int len)
        {
            var bytes = new byte[len];
            var enc = Encoding.ASCII.GetBytes(s ?? string.Empty);
            Buffer.BlockCopy(enc, 0, bytes, 0, Math.Min(enc.Length, len));
            return bytes;
        }

        // ─────────────────────────────────────────────
        // 工具
        // ─────────────────────────────────────────────
        private static void GuardBounds(byte[] buf, int off, int size)
        {
            if (off < 0 || off + size > buf.Length)
                throw new IndexOutOfRangeException(
                    $"偏移 {off}+{size} 越界 (buffer={buf.Length})");
        }
    }
}
