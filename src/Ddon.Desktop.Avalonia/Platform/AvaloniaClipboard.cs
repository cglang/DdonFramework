using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Ddon.Desktop.Platform;

namespace Ddon.Desktop.Avalonia.Platform;

public class AvaloniaClipboard : Ddon.Desktop.Platform.IClipboard
{
    public async Task<string> GetTextAsync()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime
            && lifetime.MainWindow?.Clipboard is { } cb)
        {
            return await cb.TryGetTextAsync() ?? string.Empty;
        }

        return string.Empty;
    }

    public async Task SetTextAsync(string text)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime
            && lifetime.MainWindow?.Clipboard is { } cb)
        {
            await cb.SetTextAsync(text);
        }
    }
}
