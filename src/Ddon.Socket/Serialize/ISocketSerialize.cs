using System;

namespace Ddon.Socket.Serialize
{
    public interface ISocketSerialize
    {
        string SerializeOfString<T>(T data);

        Memory<byte> SerializeOfByte<T>(T data);

        T? Deserialize<T>(string data);

        T? Deserialize<T>(Memory<byte> data);
    }
}
