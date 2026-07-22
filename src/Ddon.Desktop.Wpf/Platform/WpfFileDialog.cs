using Ddon.Desktop.Core.Platform;

namespace Ddon.Desktop.Wpf.Platform;

public class WpfFileDialog : IFileDialog
{
    public Task<string?> OpenFileAsync(string filter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = filter
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }

    public Task<string?> SaveFileAsync(string filter, string defaultName)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = filter,
            FileName = defaultName
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }
}
