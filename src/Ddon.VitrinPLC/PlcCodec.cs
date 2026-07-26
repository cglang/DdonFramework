using System;
using System.Text;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC
{
    public static class PlcCodec
    {
        public static T Read<T>(BufferSlice data, ParsedAddress addr, int stringLength = 0, EndianFormat endian = EndianFormat.ABCD)
        {
            object value = addr.DataType switch
            {
                PlcDataType.Bool => ReadBool(data, addr.ByteOffset, addr.BitIndex),
                PlcDataType.Byte => data.ReadByte(addr.ByteOffset),
                PlcDataType.Int16 => ReadInt16(data, addr.ByteOffset, endian),
                PlcDataType.UInt16 => ReadUInt16(data, addr.ByteOffset, endian),
                PlcDataType.Int32 => ReadInt32(data, addr.ByteOffset, endian),
                PlcDataType.UInt32 => ReadUInt32(data, addr.ByteOffset, endian),
                PlcDataType.Float => ReadFloat(data, addr.ByteOffset, endian),
                PlcDataType.Double => ReadDouble(data, addr.ByteOffset, endian),
                PlcDataType.String => ReadString(data, addr.ByteOffset, stringLength > 0 ? stringLength : 256),
                _ => throw new NotSupportedException($"不支持的类型: {addr.DataType}")
            };

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static byte[] Encode<T>(T value, PlcDataType type, EndianFormat endian = EndianFormat.ABCD,
            int byteOffset = 0, int bitIndex = 0, int stringLength = 0)
        {
            return type switch
            {
                PlcDataType.Bool => EncodeBool(Convert.ToBoolean(value), byteOffset, bitIndex),
                PlcDataType.Byte => new[] { Convert.ToByte(value) },
                PlcDataType.Int16 => EncodeInt16(Convert.ToInt16(value), endian),
                PlcDataType.UInt16 => EncodeUInt16(Convert.ToUInt16(value), endian),
                PlcDataType.Int32 => EncodeInt32(Convert.ToInt32(value), endian),
                PlcDataType.UInt32 => EncodeUInt32(Convert.ToUInt32(value), endian),
                PlcDataType.Float => EncodeFloat(Convert.ToSingle(value), endian),
                PlcDataType.Double => EncodeDouble(Convert.ToDouble(value), endian),
                PlcDataType.String => EncodeString(Convert.ToString(value), stringLength > 0 ? stringLength : 256),
                _ => throw new NotSupportedException($"不支持的类型: {type}")
            };
        }

        private static bool ReadBool(BufferSlice data, int byteOff, int bit)
        {
            return (data.ReadByte(byteOff) & (1 << bit)) != 0;
        }

        private static byte[] EncodeBool(bool val, int byteOff, int bit)
        {
            return new[] { val ? (byte)(1 << bit) : (byte)0 };
        }

        private static short ReadInt16(BufferSlice data, int off, EndianFormat endian)
        {
            if (endian is EndianFormat.ABCD or EndianFormat.BADC)
                return (short)((data.ReadByte(off) << 8) | data.ReadByte(off + 1));
            return (short)(data.ReadByte(off) | (data.ReadByte(off + 1) << 8));
        }

        private static ushort ReadUInt16(BufferSlice data, int off, EndianFormat endian)
        {
            if (endian is EndianFormat.ABCD or EndianFormat.BADC)
                return (ushort)((data.ReadByte(off) << 8) | data.ReadByte(off + 1));
            return (ushort)(data.ReadByte(off) | (data.ReadByte(off + 1) << 8));
        }

        private static byte[] EncodeInt16(short v, EndianFormat endian)
        {
            if (endian is EndianFormat.ABCD or EndianFormat.BADC)
                return new[] { (byte)(v >> 8), (byte)v };
            return new[] { (byte)v, (byte)(v >> 8) };
        }

        private static byte[] EncodeUInt16(ushort v, EndianFormat endian)
        {
            if (endian is EndianFormat.ABCD or EndianFormat.BADC)
                return new[] { (byte)(v >> 8), (byte)v };
            return new[] { (byte)v, (byte)(v >> 8) };
        }

        private static int ReadInt32(BufferSlice data, int off, EndianFormat endian)
        {
            return endian switch
            {
                EndianFormat.ABCD => (data.ReadByte(off) << 24) | (data.ReadByte(off + 1) << 16) | (data.ReadByte(off + 2) << 8) | data.ReadByte(off + 3),
                EndianFormat.BADC => (data.ReadByte(off + 1) << 24) | (data.ReadByte(off) << 16) | (data.ReadByte(off + 3) << 8) | data.ReadByte(off + 2),
                EndianFormat.CDAB => (data.ReadByte(off + 2) << 24) | (data.ReadByte(off + 3) << 16) | (data.ReadByte(off) << 8) | data.ReadByte(off + 1),
                EndianFormat.DCBA => data.ReadByte(off) | (data.ReadByte(off + 1) << 8) | (data.ReadByte(off + 2) << 16) | (data.ReadByte(off + 3) << 24),
                _ => throw new ArgumentOutOfRangeException(nameof(endian))
            };
        }

        private static uint ReadUInt32(BufferSlice data, int off, EndianFormat endian)
        {
            return (uint)ReadInt32(data, off, endian);
        }

        private static byte[] EncodeInt32(int v, EndianFormat endian)
        {
            return endian switch
            {
                EndianFormat.ABCD => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v },
                EndianFormat.BADC => new[] { (byte)(v >> 16), (byte)(v >> 24), (byte)v, (byte)(v >> 8) },
                EndianFormat.CDAB => new[] { (byte)(v >> 8), (byte)v, (byte)(v >> 24), (byte)(v >> 16) },
                EndianFormat.DCBA => new[] { (byte)v, (byte)(v >> 8), (byte)(v >> 16), (byte)(v >> 24) },
                _ => throw new ArgumentOutOfRangeException(nameof(endian))
            };
        }

        private static byte[] EncodeUInt32(uint v, EndianFormat endian)
        {
            return EncodeInt32((int)v, endian);
        }

        private static float ReadFloat(BufferSlice data, int off, EndianFormat endian)
        {
            var bytes = data.ReadBytes(off, 4);
            var tmp = ArrangeBytes(bytes, 4, endian);
            return BitConverter.ToSingle(tmp, 0);
        }

        private static double ReadDouble(BufferSlice data, int off, EndianFormat endian)
        {
            var bytes = data.ReadBytes(off, 8);
            var tmp = ArrangeBytes(bytes, 8, endian);
            return BitConverter.ToDouble(tmp, 0);
        }

        private static byte[] EncodeFloat(float v, EndianFormat endian)
        {
            var b = BitConverter.GetBytes(v);
            return ArrangeBytes(b, 4, endian);
        }

        private static byte[] EncodeDouble(double v, EndianFormat endian)
        {
            var b = BitConverter.GetBytes(v);
            return ArrangeBytes(b, 8, endian);
        }

        private static byte[] ArrangeBytes(byte[] src, int count, EndianFormat endian)
        {
            if (endian == EndianFormat.ABCD)
                return src;

            var dst = new byte[count];
            switch (count)
            {
                case 2 when endian is EndianFormat.DCBA or EndianFormat.CDAB:
                    dst[0] = src[1]; dst[1] = src[0];
                    break;
                case 4:
                    switch (endian)
                    {
                        case EndianFormat.DCBA:
                            dst[0] = src[3]; dst[1] = src[2]; dst[2] = src[1]; dst[3] = src[0];
                            break;
                        case EndianFormat.BADC:
                            dst[0] = src[1]; dst[1] = src[0]; dst[2] = src[3]; dst[3] = src[2];
                            break;
                        case EndianFormat.CDAB:
                            dst[0] = src[2]; dst[1] = src[3]; dst[2] = src[0]; dst[3] = src[1];
                            break;
                    }
                    break;
                case 8:
                    switch (endian)
                    {
                        case EndianFormat.DCBA:
                            for (int i = 0; i < 8; i++) dst[i] = src[7 - i];
                            break;
                        case EndianFormat.BADC:
                            for (int i = 0; i < 8; i += 2) { dst[i] = src[i + 1]; dst[i + 1] = src[i]; }
                            break;
                        case EndianFormat.CDAB:
                            for (int i = 0; i < 8; i += 4) { dst[i] = src[i + 2]; dst[i + 1] = src[i + 3]; dst[i + 2] = src[i]; dst[i + 3] = src[i + 1]; }
                            break;
                    }
                    break;
            }
            return dst;
        }

        private static string ReadString(BufferSlice data, int off, int len)
        {
            var bytes = data.ReadBytes(off, len);
            return Encoding.ASCII.GetString(bytes, 0, bytes.Length).TrimEnd('\0');
        }

        private static byte[] EncodeString(string s, int len)
        {
            var bytes = new byte[len];
            var enc = Encoding.ASCII.GetBytes(s ?? string.Empty);
            Buffer.BlockCopy(enc, 0, bytes, 0, Math.Min(enc.Length, len));
            return bytes;
        }
    }
}
