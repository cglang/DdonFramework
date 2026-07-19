using System;
using System.Text;
using Ddon.Socket.Abstractions;

namespace Ddon.OpenProtocol.Protocols
{
    public class OpenProtocolFrameProtocol : ISocketProtocol
    {
        public byte[] Encode(object message)
        {
            if (message is byte[] data)
                return data;

            if (message is string text)
                return Encoding.ASCII.GetBytes(text);

            throw new InvalidOperationException("OpenProtocolFrameProtocol requires byte[] or string payload.");
        }

        public (byte[]? Frame, int Consumed) Decode(byte[] buffer, int offset, int count)
        {
            int start = offset;
            int end = offset + count;

            while (start < end)
            {
                byte b = buffer[start];
                if (b >= (byte)'0' && b <= (byte)'9')
                    break;
                start++;
            }

            int skipped = start - offset;

            int remaining = end - start;
            if (remaining < 4)
                return (null, skipped);

            if (!TryParseLength(buffer, start, out int frameLength))
                return (null, skipped + 1);

            if (frameLength <= 0 || frameLength > 65535)
                return (null, skipped + 1);

            int totalLength = frameLength;
            if (remaining < totalLength)
                return (null, skipped);

            var frame = new byte[totalLength];
            Array.Copy(buffer, start, frame, 0, totalLength);
            return (frame, skipped + totalLength);
        }

        private static bool TryParseLength(byte[] buffer, int offset, out int length)
        {
            length = 0;
            for (int i = 0; i < 4; i++)
            {
                byte b = buffer[offset + i];
                if (b < (byte)'0' || b > (byte)'9')
                    return false;
                length = length * 10 + (b - (byte)'0');
            }
            return true;
        }
    }
}
