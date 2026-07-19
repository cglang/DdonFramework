namespace Ddon.Desktop.Platform;

public interface IProcess
{
    Task<int> StartAsync(string fileName, string? arguments = null);
}
