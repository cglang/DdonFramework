using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Ddon.Socket.Serialize
{
    public class JsonSocketSerialize : ISocketSerialize
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public string SerializeOfString<T>(T data)
        {
            return JsonSerializer.Serialize(data, Options);
        }

        public ReadOnlyMemory<byte> SerializeOfByte<T>(T data)
        {
            return SerializeOfString(data).GetBytes();
        }

        public T? Deserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, Options);
        }

        public T? Deserialize<T>(ReadOnlyMemory<byte> data)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data.Span), Options);
        }
    }
}
