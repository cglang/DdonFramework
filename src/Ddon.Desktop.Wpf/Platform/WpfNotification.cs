using Ddon.Desktop.Core.Platform;

namespace Ddon.Desktop.Wpf.Platform;

public class WpfNotification : INotification
{
    public Task ShowAsync(string title, string message)
    {
        System.Windows.MessageBox.Show(message, title);
        return Task.CompletedTask;
    }
}
