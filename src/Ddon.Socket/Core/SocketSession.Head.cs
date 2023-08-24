using System;
using Ddon.Socket.Utility;

namespace Ddon.Socket.Core;

public partial class SocketSession
{
    private readonly struct Head
    {
        public int Length { get; }
        public DataType Type { get; }

        public Head(Memory<byte> bytes)
        {
            Length = BitConverter.ToInt32(bytes.Span[..sizeof(int)]);
            Type = (DataType)bytes.Span[sizeof(int)];
        }

        public const int HeadLength = sizeof(int) + sizeof(DataType);
    }
}
