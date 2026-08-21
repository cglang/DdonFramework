using System.Text.Json;
using Ddon.Desktop.Core.Protocol;

namespace Ddon.Desktop.Core.Transport;

public class WebViewTransport : ITransport
{
    private readonly Dictionary<string, Delegate> _handlers = new();

    public Func<string, object?, Task<object?>>? OnInvoke { get; set; }
    public dynamic? ChromeWebView { get; set; }

    public async Task PublishAsync(string eventName, object? data = null)
    {
        PostMessage("event", new BridgeEvent { Name = eventName, Data = data });
        await Task.CompletedTask;
    }

    public void On<T>(string eventName, Action<T> handler)
    {
        _handlers[eventName] = handler;
    }

    public void Off(string eventName)
    {
        _handlers.Remove(eventName);
    }

    public async Task HandleMessage(string messageJson)
    {
        using var doc = JsonDocument.Parse(messageJson);
        var root = doc.RootElement;
        var type = root.GetProperty("type").GetString();

        switch (type)
        {
            case "invoke" when OnInvoke is not null:
                await HandleIncomingInvoke(root.GetProperty("data"));
                break;

            case "event":
                HandleIncomingEvent(root.GetProperty("data"));
                break;
        }
    }

    private async Task HandleIncomingInvoke(JsonElement data)
    {
        var request = JsonSerializer.Deserialize<BridgeRequest>(data.GetRawText());
        if (request is null) return;

        try
        {
            var result = await OnInvoke!(request.Method, request.Payload);
            PostMessage("response", new BridgeResponse
            {
                Id = request.Id,
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            PostMessage("response", new BridgeResponse
            {
                Id = request.Id,
                Success = false,
                Error = ex.Message
            });
        }
    }

    private void HandleIncomingEvent(JsonElement data)
    {
        var bridgeEvent = JsonSerializer.Deserialize<BridgeEvent>(data.GetRawText());
        if (bridgeEvent is null) return;

        if (_handlers.TryGetValue(bridgeEvent.Name, out var handler))
        {
            var targetType = handler.GetType().GetGenericArguments()[1];
            var payload = ConvertPayload(bridgeEvent.Data, targetType);
            handler.DynamicInvoke(payload);
        }
    }

    private void PostMessage(string type, object data)
    {
        var message = JsonSerializer.Serialize(new { type, data });
        ChromeWebView?.PostWebMessageAsString(message);
    }

    private static object? ConvertPayload(object? data, Type targetType)
    {
        return data switch
        {
            JsonElement je => JsonSerializer.Deserialize(je.GetRawText(), targetType, _jsonOptions),
            not null => JsonSerializer.Deserialize(JsonSerializer.Serialize(data), targetType, _jsonOptions),
            null => null
        };
    }

    public Task InjectBridgeAsync()
    {
        return Task.CompletedTask;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
