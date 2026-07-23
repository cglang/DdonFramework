using Ddon.Desktop.Core.Platform;

namespace Ddon.Desktop.Wpf.Platform;

public class WpfClipboard : IClipboard
{
    public Task<string> GetTextAsync()
    {
        return Task.FromResult(System.Windows.Clipboard.GetText() ?? string.Empty);
    }

    public Task SetTextAsync(string text)
    {
        System.Windows.Clipboard.SetText(text);
        return Task.CompletedTask;
    }
}
