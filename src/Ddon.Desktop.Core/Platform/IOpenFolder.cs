namespace Ddon.Desktop.Platform;

public interface IOpenFolder
{
    Task<string?> OpenFolderAsync();
}
