using System;
using System.Text;
using Ddon.Socket.Abstractions;

namespace Ddon.Socket.Protocols
{
    public class LineProtocol : ISocketProtocol
    {
        private readonly Encoding _encoding;

        public LineProtocol() : this(Encoding.UTF8)
        {
        }

        public LineProtocol(Encoding encoding)
        {
            _encoding = encoding;
        }

        public byte[] Encode(object message)
        {
            var text = message?.ToString() ?? string.Empty;
            return _encoding.GetBytes(text + "\n");
        }

        public (byte[]? Frame, int Consumed) Decode(byte[] buffer, int offset, int count)
        {
            var end = offset + count;
            for (int i = offset; i < end; i++)
            {
                if (buffer[i] == '\n')
                {
                    var frameLength = i - offset + 1;
                    var frame = new byte[frameLength];
                    Array.Copy(buffer, offset, frame, 0, frameLength);
                    return (frame, frameLength);
                }
            }
            return (null, 0);
        }
    }
}
