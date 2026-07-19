using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using Ddon.Desktop.Protocol;

namespace Ddon.Desktop.Transport;

public class AvaloniaWebViewTransport : ITransport
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Dictionary<string, Delegate> _handlers = new();
    private readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests = new();

    public NativeWebView? WebView { get; set; }
    public Func<string, object?, Task<object?>>? OnInvoke { get; set; }

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

        await PostMessage("invoke", request);

        var resultJson = await tcs.Task;
        var response = JsonSerializer.Deserialize<BridgeResponse>(resultJson, _jsonOptions)!;

        if (!response.Success)
            throw new InvalidOperationException(response.Error ?? "Bridge invoke failed");

        return DeserializeData<T>(response.Data);
    }

    public async Task PublishAsync(string eventName, object? data = null)
    {
        await PostMessage("event", new BridgeEvent { Name = eventName, Data = data });
    }

    public void On<T>(string eventName, Action<T> handler)
    {
        _handlers[eventName] = handler;
    }

    public void Off(string eventName)
    {
        _handlers.Remove(eventName);
    }

    public async Task HandleMessage(string message)
    {
        using var doc = JsonDocument.Parse(message);
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

            case "bridgeReady":
                break;
        }
    }

    public async Task InjectBridgeAsync()
    {
        var script = GetBridgeJavaScript();
        await (WebView?.InvokeScript(script) ?? Task.CompletedTask);
    }

    public void HandleResponse(string responseJson)
    {
        var response = JsonSerializer.Deserialize<BridgeResponse>(responseJson, _jsonOptions);
        if (response is not null && _pendingRequests.TryGetValue(response.Id, out var tcs))
        {
            tcs.TrySetResult(responseJson);
            _pendingRequests.Remove(response.Id);
        }
    }

    private async Task HandleIncomingInvoke(JsonElement data)
    {
        var request = JsonSerializer.Deserialize<BridgeRequest>(data.GetRawText(), _jsonOptions);
        if (request is null) return;

        try
        {
            var result = await OnInvoke!(request.Method, request.Payload);
            await PostMessage("response", new BridgeResponse
            {
                Id = request.Id,
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            await PostMessage("response", new BridgeResponse
            {
                Id = request.Id,
                Success = false,
                Error = ex.Message
            });
        }
    }

    private void HandleIncomingEvent(JsonElement data)
    {
        var bridgeEvent = JsonSerializer.Deserialize<BridgeEvent>(data.GetRawText(), _jsonOptions);
        if (bridgeEvent is null) return;

        if (_handlers.TryGetValue(bridgeEvent.Name, out var handler))
        {
            var targetType = handler.GetType().GetGenericArguments()[1];
            var payload = ConvertPayload(bridgeEvent.Data, targetType);
            handler.DynamicInvoke(payload);
        }
    }

    private async Task PostMessage(string type, object data)
    {
        if (WebView is null) return;

        var json = JsonSerializer.Serialize(new { type, data }, _jsonOptions);
        var safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        await WebView.InvokeScript($"window.__bridgeReceive(atob('{safe}'))");
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
            not null => JsonSerializer.Deserialize(JsonSerializer.Serialize(data, _jsonOptions), targetType, _jsonOptions),
            null => null
        };
    }

    private static string GetBridgeJavaScript()
    {
        return """
(function() {
  window.__bridgeCallbacks = {};

  window.__bridgeReceive = function(data) {
    try {
      var msg = typeof data === 'string' ? JSON.parse(data) : data;
      if (msg.type === 'response') {
        var resolve = window.__bridgeCallbacks[msg.data.id];
        if (resolve) {
          resolve(msg.data);
          delete window.__bridgeCallbacks[msg.data.id];
        }
      } else if (msg.type === 'event') {
        console.log('[bridge] event:', msg.data.name);
      }
    } catch(e) {
      console.error('[bridge] receive error:', e);
    }
  };

  window.invokeCSharpAction = function(body) {
    try {
      var msg = JSON.parse(body);
      if (msg.type === 'invoke') {
        window.__bridgeCallbacks[msg.data.id] = function(response) {
          invokeCSharpAction(JSON.stringify({ type: 'response', data: response }));
        };
      }
    } catch(e) {}
  };

  invokeCSharpAction(JSON.stringify({ type: 'bridgeReady', data: {} }));
  console.log('[bridge] injected');
})();
""";
    }
}
