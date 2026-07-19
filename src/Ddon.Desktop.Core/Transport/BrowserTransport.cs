using System.Net.Http.Json;
using System.Text.Json;
using Ddon.Desktop.Protocol;

namespace Ddon.Desktop.Transport;

public class BrowserTransport : ITransport
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, Delegate> _handlers = new();

    public BrowserTransport(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> InvokeAsync<T>(string method, object? payload = null)
    {
        var request = new BridgeRequest
        {
            Id = Guid.NewGuid().ToString(),
            Method = method,
            Payload = payload
        };

        var response = await _httpClient.PostAsJsonAsync("/api/bridge/invoke", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BridgeResponse>();
        if (result is null || !result.Success)
            throw new InvalidOperationException(result?.Error ?? "Bridge invoke failed");

        return DeserializeData<T>(result.Data);
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

    private static T DeserializeData<T>(object? data)
    {
        if (data is JsonElement je)
            return JsonSerializer.Deserialize<T>(je.GetRawText(), _jsonOptions)!;
        return (T)data!;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
