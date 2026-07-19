namespace Ddon.Desktop.Bridge;

public interface IBridgeDispatcher
{
    Task<object?> DispatchAsync(string method, object? payload);
}
