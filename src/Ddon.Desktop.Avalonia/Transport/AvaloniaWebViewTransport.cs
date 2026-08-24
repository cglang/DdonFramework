using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Avalonia.Controls;
using Ddon.Desktop.Core.Bridge;
using Ddon.Desktop.Core.Protocol;
using Ddon.Desktop.Core.Transport;
using Microsoft.Extensions.Logging;

namespace Ddon.Desktop.Avalonia.Transport;

public class AvaloniaWebViewTransport : ITransport
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>日志专用序列化选项:中文等非 ASCII 字符不转义,便于阅读排查</summary>
    private static readonly JsonSerializerOptions _logJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Delegate> _handlers = new();
    private readonly IBridgeDispatcher _bridgeDispatcher;
    private readonly ILogger<AvaloniaWebViewTransport> _logger;

    public NativeWebView? WebView { get; set; }

    public AvaloniaWebViewTransport(IBridgeDispatcher bridgeDispatcher, ILogger<AvaloniaWebViewTransport> logger)
    {
        _bridgeDispatcher = bridgeDispatcher;
        _logger = logger;
    }


    public async Task PublishAsync(string eventName, object? data = null)
    {
        _logger.LogDebug("发送消息: Native → WebView | 操作: event | 事件名: {EventName} | 数据: {Data}",
            eventName, FormatPayload(data));

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
        // args.Body 一次性反序列化为具体的消息对象,避免反复用 JsonDocument 取数
        var envelope = JsonSerializer.Deserialize<WebViewMessage>(message, _jsonOptions);
        if (envelope is null || string.IsNullOrEmpty(envelope.Type))
        {
            _logger.LogWarning("收到消息: WebView → Native | 操作: 未知 | 消息无法解析");
            return;
        }

        switch (envelope.Type)
        {
            case "invoke":
                await HandleIncomingInvoke(envelope.Data);
                break;

            case "event":
                HandleIncomingEvent(envelope.Data);
                break;

            default:
                _logger.LogWarning("收到消息: WebView → Native | 操作: {Operation} | 未注册的操作类型", envelope.Type);
                break;
        }
    }

    public async Task InjectBridgeAsync()
    {
        var script = GetBridgeJavaScript();
        try
        {
            await (WebView?.InvokeScript(script) ?? Task.CompletedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebView 执行脚本错误: {Script}", script);
        }
    }

    private async Task HandleIncomingInvoke(JsonElement? data)
    {
        if (data is not { } element) return;
        var request = JsonSerializer.Deserialize<BridgeRequest>(element, _jsonOptions);
        if (request is null) return;

        _logger.LogDebug("收到消息: WebView → Native | 操作: invoke | 请求ID: {RequestId} | 方法: {Method} | 参数: {Payload}",
            request.Id, request.Method, FormatPayload(request.Payload));

        try
        {
            var result = await _bridgeDispatcher.DispatchAsync(request.Method, request.Payload);
            _logger.LogDebug("发送消息: Native → WebView | 操作: response | 请求ID: {RequestId} | 方法: {Method} | 结果: 成功 | 返回值: {Data}",
                request.Id, request.Method, FormatPayload(result));
            await PostMessage("response", new BridgeResponse { Id = request.Id, Success = true, Data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError("发送消息: Native → WebView | 操作: response | 请求ID: {RequestId} | 方法: {Method} | 结果: 失败 | 错误: {Error}",
                request.Id, request.Method, ex.Message);
            await PostMessage("response", new BridgeResponse { Id = request.Id, Success = false, Error = ex.Message });
        }
    }

    private void HandleIncomingEvent(JsonElement? data)
    {
        if (data is not { } element) return;
        var bridgeEvent = JsonSerializer.Deserialize<BridgeEvent>(element, _jsonOptions);
        if (bridgeEvent is null) return;

        _logger.LogDebug("收到消息: WebView → Native | 操作: event | 事件名: {EventName} | 数据: {Data}",
            bridgeEvent.Name, FormatPayload(bridgeEvent.Data));

        if (_handlers.TryGetValue(bridgeEvent.Name, out var handler))
        {
            var targetType = handler.GetType().GetGenericArguments()[1];
            var payload = ConvertPayload(bridgeEvent.Data, targetType);
            handler.DynamicInvoke(payload);
        }
        else
        {
            _logger.LogWarning("收到消息: WebView → Native | 操作: event | 事件名: {EventName} | 无订阅者", bridgeEvent.Name);
        }
    }

    private async Task PostMessage(string type, object data)
    {
        if (WebView is null) return;

        var json = JsonSerializer.Serialize(new { type, data }, _jsonOptions);
        var safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        try
        {
            await WebView.InvokeScript($"window.ui.onMessage(atob('{safe}'))");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息失败: Native → WebView | 类型: {Type}", type);
        }
    }

    private static object? ConvertPayload(object? data, Type targetType)
    {
        return data switch
        {
            JsonElement je => JsonSerializer.Deserialize(je.GetRawText(), targetType, _jsonOptions),
            not null => JsonSerializer.Deserialize(JsonSerializer.Serialize(data, _jsonOptions), targetType,
                _jsonOptions),
            null => null
        };
    }

    /// <summary>将参数/返回值格式化为可读文本</summary>
    private static string FormatPayload(object? payload)
    {
        if (payload is null)
            return "(空)";

        try
        {
            return JsonSerializer.Serialize(payload, _logJsonOptions);
        }
        catch
        {
            return payload.ToString() ?? "(无法序列化)";
        }
    }

    private static string GetBridgeJavaScript()
    {
        return "(function() { injectBridge() })()";
    }
}
