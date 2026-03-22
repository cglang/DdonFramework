using System;

namespace Ddon.IoTDevice.Extensions
{
    public static class IotNoteExtensions
    {
        public static T Read<T>(this IIoTNode<T> node) where T : struct
        {
            var client = node.Module.IotDevice.Client;

            switch (typeof(T))
            {
                case Type t when t == typeof(bool):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(bool), true);
                        return (T)(object)BitConverter.ToBoolean(bytes.Value, 0);
                    }

                case Type t when t == typeof(byte):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(byte));
                        return (T)(object)bytes.Value[0];
                    }

                case Type t when t == typeof(sbyte):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(sbyte));
                        return (T)(object)(sbyte)bytes.Value[0];
                    }

                case Type t when t == typeof(short):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(short));
                        return (T)(object)BitConverter.ToInt16(bytes.Value, 0);
                    }

                case Type t when t == typeof(ushort):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(ushort));
                        return (T)(object)BitConverter.ToUInt16(bytes.Value, 0);
                    }

                case Type t when t == typeof(int):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(int));
                        return (T)(object)BitConverter.ToInt32(bytes.Value, 0);
                    }

                case Type t when t == typeof(uint):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(uint));
                        return (T)(object)BitConverter.ToUInt32(bytes.Value, 0);
                    }

                case Type t when t == typeof(long):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(long));
                        return (T)(object)BitConverter.ToInt64(bytes.Value, 0);
                    }

                case Type t when t == typeof(ulong):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(ulong));
                        return (T)(object)BitConverter.ToUInt64(bytes.Value, 0);
                    }

                case Type t when t == typeof(float):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(float));
                        return (T)(object)BitConverter.ToSingle(bytes.Value, 0);
                    }

                case Type t when t == typeof(double):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(double));
                        return (T)(object)BitConverter.ToDouble(bytes.Value, 0);
                    }

                case Type t when t == typeof(char):
                    {
                        var bytes = client.ReadBytes(node.Address, sizeof(char));
                        return (T)(object)BitConverter.ToChar(bytes.Value, 0);
                    }

                default:
                    throw new NotSupportedException($"Type {typeof(T)} is not supported.");
            }
        }

        public static void Write<T>(this IIoTNode<T> node, T value)
        {
            var client = node.Module.IotDevice.Client;

            switch (typeof(T))
            {
                case Type t when t == typeof(bool):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((bool)(object)value));
                        return;
                    }

                case Type t when t == typeof(byte):
                    {
                        client.WriteBytes(node.Address, new[] { (byte)(object)value });
                        return;
                    }

                case Type t when t == typeof(sbyte):
                    {
                        client.WriteBytes(node.Address, new[] { unchecked((byte)(sbyte)(object)value) });
                        return;
                    }

                case Type t when t == typeof(short):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((short)(object)value));
                        return;
                    }

                case Type t when t == typeof(ushort):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((ushort)(object)value));
                        return;
                    }

                case Type t when t == typeof(int):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((int)(object)value));
                        return;
                    }

                case Type t when t == typeof(uint):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((uint)(object)value));
                        return;
                    }

                case Type t when t == typeof(long):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((long)(object)value));
                        return;
                    }

                case Type t when t == typeof(ulong):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((ulong)(object)value));
                        return;
                    }

                case Type t when t == typeof(float):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((float)(object)value));
                        return;
                    }

                case Type t when t == typeof(double):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((double)(object)value));
                        return;
                    }

                case Type t when t == typeof(char):
                    {
                        client.WriteBytes(node.Address, BitConverter.GetBytes((char)(object)value));
                        return;
                    }

                default:
                    throw new NotSupportedException($"Type {typeof(T)} is not supported.");
            }
        }

    }
}
