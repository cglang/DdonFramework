using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Ddon.Socket.Serialize
{
    public class SerializeHelper
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public static string JsonSerialize<T>(T data)
        {
            return JsonSerializer.Serialize(data, Options);
        }

        public static T? JsonDeserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, Options);
        }

        public static T? JsonDeserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data, Options);
        }

        public static T? JsonDeserialize<T>(Memory<byte> data)
        {
            return JsonSerializer.Deserialize<T>(data.Span, Options);
        }
    }
}
