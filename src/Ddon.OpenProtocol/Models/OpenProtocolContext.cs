using System;
using System.Collections.Generic;
using System.Text;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Models
{
    public class OpenProtocolContext
    {
        public string ConnectionName { get; set; } = string.Empty;

        public byte[] Buffer { get; set; } = Array.Empty<byte>();

        public int Length { get; set; }

        public DateTime ReceiveTime { get; set; } = DateTime.UtcNow;

        public Mid? ParsedMessage { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new();

        public string GetString(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            return encoding.GetString(Buffer, 0, Length);
        }
    }
}
