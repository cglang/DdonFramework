namespace Ddon.Desktop.Core.Platform;

public interface IOpenFolder
{
    Task<string?> OpenFolderAsync();
}
