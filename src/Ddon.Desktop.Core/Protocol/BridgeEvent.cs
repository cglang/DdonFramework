using System.Text.Json.Serialization;

namespace Ddon.Desktop.Core.Protocol;

public class BridgeEvent
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
