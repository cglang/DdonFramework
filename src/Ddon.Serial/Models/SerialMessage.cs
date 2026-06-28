using System;
using System.Collections.Generic;

namespace Ddon.Serial.Models
{
    public class SerialMessage
    {
        public string PortName { get; set; } = string.Empty;

        public byte[] Buffer { get; set; } = Array.Empty<byte>();

        public int Length { get; set; }

        public DateTime ReceiveTime { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
