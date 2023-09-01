using System;
using System.Text;
using MemoryPack;

namespace Ddon.Socket.Serialize
{
    public class MemoryPackSerialize : ISocketSerialize
    {
        public T? Deserialize<T>(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }

        public T? Deserialize<T>(ReadOnlyMemory<byte> data)
        {
            return MemoryPackSerializer.Deserialize<T>(data.Span);
        }

        public ReadOnlyMemory<byte> SerializeOfByte<T>(T data)
        {
            return MemoryPackSerializer.Serialize(data);
        }

        public string SerializeOfString<T>(T data)
        {
            var bytes = MemoryPackSerializer.Serialize(data);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
