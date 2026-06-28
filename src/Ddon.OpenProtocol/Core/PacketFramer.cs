using System;
using System.Collections.Generic;
using System.Text;

namespace Ddon.OpenProtocol.Core
{
    public class PacketFramer
    {
        private readonly List<byte> _buffer = new();
        private readonly object _lock = new();

        public void Feed(byte[] data)
        {
            lock (_lock)
            {
                _buffer.AddRange(data);
            }
        }

        public bool TryReadPacket(out byte[]? packet)
        {
            packet = null;

            lock (_lock)
            {
                while (_buffer.Count > 0)
                {
                    byte first = _buffer[0];
                    if (first >= (byte)'0' && first <= (byte)'9')
                        break;
                    _buffer.RemoveAt(0);
                }

                if (_buffer.Count < 4)
                    return false;

                string lenStr = Encoding.ASCII.GetString(
                    _buffer.GetRange(0, 4).ToArray());

                if (!TryParseLength(lenStr, out int frameLength))
                {
                    _buffer.RemoveAt(0);
                    return false;
                }

                if (frameLength <= 0 || frameLength > 65535)
                {
                    _buffer.RemoveAt(0);
                    return false;
                }

                if (_buffer.Count < frameLength)
                    return false;

                packet = _buffer.GetRange(0, frameLength).ToArray();
                _buffer.RemoveRange(0, frameLength);
                return true;
            }
        }

        private static bool TryParseLength(string text, out int length)
        {
            length = 0;
            if (text.Length != 4) return false;

            foreach (char c in text)
            {
                if (c < '0' || c > '9')
                    return false;
                length = length * 10 + (c - '0');
            }

            return true;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _buffer.Clear();
            }
        }
    }
}
