using System.Text.Json.Serialization;

namespace Ddon.Desktop.Core.Protocol;

public class BridgeError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
