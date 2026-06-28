using System;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Abstractions
{
    public interface IOpenProtocolProtocol
    {
        byte[] Serialize(Mid mid);

        Mid? Deserialize(byte[] packet);

        void RegisterCustomMid<T>(int? midNumber = null) where T : Mid;

        bool TryMapResponse(int requestMid, int responseMid);
    }
}
