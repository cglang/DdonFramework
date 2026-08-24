using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ddon.Desktop.Core.Protocol;

/// <summary>
/// WebView 发送给原生端的消息信封,与前端 bridge 中发送的 { type, data } 结构一一对应。
/// args.Body 反序列化为此对象后即可按类型分发,无需反复用 JsonDocument 取数。
/// </summary>
public class WebViewMessage
{
    /// <summary>消息类型:invoke(桥接方法调用) / event(事件推送)</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>消息负载:invoke 时为 BridgeRequest,event 时为 BridgeEvent</summary>
    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }
}
