namespace Ddon.Desktop.Platform;

public interface IShell
{
    Task OpenUrlAsync(string url);
    Task OpenFileAsync(string filePath);
}
