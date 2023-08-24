using System;

namespace Ddon.Socket.Serialize
{
    public interface ISocketSerialize
    {
        string SerializeOfString<T>(T data);

        ReadOnlyMemory<byte> SerializeOfByte<T>(T data);

        T? Deserialize<T>(string data);

        T? Deserialize<T>(ReadOnlyMemory<byte> data);
    }
}
