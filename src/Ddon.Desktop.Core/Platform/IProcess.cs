namespace Ddon.Desktop.Core.Platform;

public interface IProcess
{
    Task<int> StartAsync(string fileName, string? arguments = null);
}
