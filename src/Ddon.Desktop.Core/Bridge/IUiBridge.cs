namespace Ddon.Desktop.Core.Bridge;

public interface IUiBridge
{
    Task<T> InvokeAsync<T>(string method, object? payload = null);
    Task PublishAsync<T>(T eventData) where T : class;
    void On<T>(string eventName, Action<T> handler);
    void Off(string eventName);
}
