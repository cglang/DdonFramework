using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Ddon.Socket.Serialize
{
    public interface ISocketSerialize
    {
        string SerializeOfString<T>(T data);

        Memory<byte> SerializeOfByte<T>(T data);

        T? Deserialize<T>(string data);

        T? Deserialize<T>(Memory<byte> data);
    }

    public class SocketSerialize : ISocketSerialize
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

        public Memory<byte> SerializeOfByte<T>(T data)
        {
            return Encoding.UTF8.GetBytes(SerializeOfString(data));
        }

        public T? Deserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, Options);
        }

        public T? Deserialize<T>(Memory<byte> data)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data.Span), Options);
        }
    }
}
