namespace Ddon.Desktop.Core.Bridge;

public interface IBridgeDispatcher
{
    Task<object?> DispatchAsync(string method, object? payload);
}
