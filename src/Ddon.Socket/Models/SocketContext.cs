using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ddon.Socket.Models
{
    public class SocketContext
    {
        public string ConnectionId { get; set; } = string.Empty;

        public EndPoint? RemoteEndPoint { get; set; }

        public EndPoint? LocalEndPoint { get; set; }

        public byte[] Buffer { get; set; } = Array.Empty<byte>();

        public int Length { get; set; }

        public DateTime ReceiveTime { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public object? ParsedMessage { get; set; }

        public string GetString(Encoding? encoding = null, bool trimNewLine = true)
        {
            encoding ??= Encoding.UTF8;
            var text = encoding.GetString(Buffer, 0, Length);
            return trimNewLine ? text.TrimEnd('\r', '\n') : text;
        }
    }
}
