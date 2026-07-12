using System;
using System.Text;
using Ddon.Socket.Abstractions;

namespace Ddon.Socket.Protocols
{
    /// <summary>
    /// 支持 \r、\n、\r\n 三种换行符
    /// </summary>
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
            // 默认发送使用 \n
            return _encoding.GetBytes(text + "\n");
        }

        public (byte[]? Frame, int Consumed) Decode(byte[] buffer, int offset, int count)
        {
            ReadOnlySpan<byte> span = buffer.AsSpan(offset, count);

            int index = span.IndexOfAny((byte)'\r', (byte)'\n');
            if (index < 0)
                return (null, 0);

            int consumed;

            if (span[index] == (byte)'\r')
            {
                if (index + 1 < span.Length && span[index + 1] == (byte)'\n')
                {
                    consumed = index + 2;
                }
                else
                {
                    consumed = index + 1;
                }
            }
            else
            {
                consumed = index + 1;
            }

            return (span.Slice(0, consumed).ToArray(), consumed);
        }
    }
}
