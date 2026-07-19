namespace Ddon.Desktop.Transport;

public interface ITransport
{
    Task<T> InvokeAsync<T>(string method, object? payload = null);
    Task PublishAsync(string eventName, object? data = null);
    void On<T>(string eventName, Action<T> handler);
    void Off(string eventName);
}
