using System;
using Ddon.Socket.Abstractions;

namespace Ddon.Socket.Protocols
{
    public class LengthPrefixProtocol : ISocketProtocol
    {
        private readonly int _prefixLength;

        public LengthPrefixProtocol() : this(4)
        {
        }

        public LengthPrefixProtocol(int prefixLength)
        {
            _prefixLength = prefixLength;
        }

        public byte[] Encode(object message)
        {
            if (message is byte[] data)
            {
                var result = new byte[_prefixLength + data.Length];
                var lengthBytes = BitConverter.GetBytes(data.Length);
                Array.Copy(lengthBytes, 0, result, 0, _prefixLength);
                Array.Copy(data, 0, result, _prefixLength, data.Length);
                return result;
            }

            throw new InvalidOperationException("LengthPrefixProtocol requires byte[] payload.");
        }

        public (byte[]? Frame, int Consumed) Decode(byte[] buffer, int offset, int count)
        {
            if (count < _prefixLength)
                return (null, 0);

            var bodyLength = BitConverter.ToInt32(buffer, offset);

            if (bodyLength <= 0 || bodyLength > 1024 * 1024)
                return (null, 0);

            var totalLength = _prefixLength + bodyLength;

            if (count < totalLength)
                return (null, 0);

            var frame = new byte[totalLength];
            Array.Copy(buffer, offset, frame, 0, totalLength);
            return (frame, totalLength);
        }
    }
}
