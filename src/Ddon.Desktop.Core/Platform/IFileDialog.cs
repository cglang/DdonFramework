namespace Ddon.Desktop.Core.Platform;

public interface IFileDialog
{
    Task<string?> OpenFileAsync(string filter);
    Task<string?> SaveFileAsync(string filter, string defaultName);
}
