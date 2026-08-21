using System.Net.Http.Json;
using Ddon.Desktop.Core.Protocol;

namespace Ddon.Desktop.Core.Transport;

public class BrowserTransport : ITransport
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, Delegate> _handlers = new();

    public BrowserTransport(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task PublishAsync(string eventName, object? data = null)
    {
        await _httpClient.PostAsJsonAsync("/api/bridge/event", new BridgeEvent
        {
            Name = eventName,
            Data = data
        });
    }

    public void On<T>(string eventName, Action<T> handler)
    {
        _handlers[eventName] = handler;
    }

    public void Off(string eventName)
    {
        _handlers.Remove(eventName);
    }

    public Task InjectBridgeAsync()
    {
        return Task.CompletedTask;
    }

    public Task HandleMessage(string message)
    {
        return Task.CompletedTask;
    }
}
