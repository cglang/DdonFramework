namespace Ddon.Desktop.Core.Transport;

public interface ITransport
{
    Task InjectBridgeAsync();

    Task HandleMessage(string message);

    Task PublishAsync(string eventName, object? data = null);

    void On<T>(string eventName, Action<T> handler);
    
    void Off(string eventName);
}
