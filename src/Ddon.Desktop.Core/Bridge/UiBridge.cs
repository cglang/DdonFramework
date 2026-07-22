using Ddon.Desktop.Core.Platform;
using Ddon.Desktop.Core.Transport;

namespace Ddon.Desktop.Core.Bridge;

public class UiBridge : IUiBridge
{
    private readonly ITransport _transport;
    private readonly Dictionary<string, IPlatformService> _platformServices = new();

    public UiBridge(ITransport transport)
    {
        _transport = transport;
    }

    public Task<T> InvokeAsync<T>(string method, object? payload = null)
    {
        return _transport.InvokeAsync<T>(method, payload);
    }

    public Task PublishAsync<T>(T eventData) where T : class
    {
        var eventName = typeof(T).Name;
        return _transport.PublishAsync(eventName, eventData);
    }

    public void On<T>(string eventName, Action<T> handler)
    {
        _transport.On(eventName, handler);
    }

    public void Off(string eventName)
    {
        _transport.Off(eventName);
    }

    public void RegisterPlatformService<T>(string key, T service) where T : class, IPlatformService
    {
        _platformServices[key] = service;
    }

    public T? GetPlatformService<T>(string key) where T : class, IPlatformService
    {
        return _platformServices.TryGetValue(key, out var service) ? service as T : null;
    }
}
