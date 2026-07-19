using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Ddon.Desktop.Platform;

namespace Ddon.Desktop.Avalonia.Platform;

public class AvaloniaFileDialog : IFileDialog
{
    public async Task<string?> OpenFileAsync(string filter)
    {
        var provider = GetStorageProvider();
        if (provider is null) return null;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = ParseFilter(filter)
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> SaveFileAsync(string filter, string defaultName)
    {
        var provider = GetStorageProvider();
        if (provider is null) return null;

        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = defaultName,
            FileTypeChoices = ParseFilter(filter)
        });

        return file?.TryGetLocalPath();
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime
            && lifetime.MainWindow is { } window)
        {
            return window.StorageProvider;
        }

        return null;
    }

    private static List<FilePickerFileType> ParseFilter(string filter)
    {
        var parts = filter.Split('|');
        if (parts.Length < 2) return [];

        var patterns = parts[1]
            .Split(';')
            .Select(e => e.Trim())
            .Where(e => e.Length > 0)
            .ToList();

        return
        [
            new FilePickerFileType(parts[0])
            {
                Patterns = patterns
            }
        ];
    }
}
