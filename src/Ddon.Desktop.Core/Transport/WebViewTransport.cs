using System.Text.Json;
using Ddon.Desktop.Protocol;

namespace Ddon.Desktop.Transport;

public class WebViewTransport : ITransport
{
    private readonly Dictionary<string, Delegate> _handlers = new();
    private readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests = new();

    public Func<string, object?, Task<object?>>? OnInvoke { get; set; }
    public dynamic? ChromeWebView { get; set; }

    public async Task<T> InvokeAsync<T>(string method, object? payload = null)
    {
        var request = new BridgeRequest
        {
            Id = Guid.NewGuid().ToString(),
            Method = method,
            Payload = payload
        };

        var tcs = new TaskCompletionSource<string>();
        _pendingRequests[request.Id] = tcs;

        PostMessage("invoke", request);

        var resultJson = await tcs.Task;
        var response = JsonSerializer.Deserialize<BridgeResponse>(resultJson)!;

        if (!response.Success)
            throw new InvalidOperationException(response.Error ?? "Bridge invoke failed");

        return DeserializeData<T>(response.Data);
    }

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
            case "response":
                HandleResponse(root.GetProperty("data").GetRawText());
                break;

            case "invoke" when OnInvoke is not null:
                await HandleIncomingInvoke(root.GetProperty("data"));
                break;

            case "event":
                HandleIncomingEvent(root.GetProperty("data"));
                break;
        }
    }

    public void HandleResponse(string responseJson)
    {
        var response = JsonSerializer.Deserialize<BridgeResponse>(responseJson);
        if (response is not null && _pendingRequests.TryGetValue(response.Id, out var tcs))
        {
            tcs.TrySetResult(responseJson);
            _pendingRequests.Remove(response.Id);
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

    private static T DeserializeData<T>(object? data)
    {
        if (data is JsonElement je)
            return JsonSerializer.Deserialize<T>(je.GetRawText(), _jsonOptions)!;
        return (T)data!;
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

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
