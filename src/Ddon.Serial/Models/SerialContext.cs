using System;
using System.Collections.Generic;
using System.Text;

namespace Ddon.Serial.Models
{
    public class SerialContext
    {
        public string PortName { get; set; } = string.Empty;

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
