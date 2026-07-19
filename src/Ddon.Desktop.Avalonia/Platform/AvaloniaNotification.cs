using Ddon.Desktop.Platform;

namespace Ddon.Desktop.Avalonia.Platform;

public class AvaloniaNotification : INotification
{
    public Task ShowAsync(string title, string message)
    {
        System.Diagnostics.Debug.WriteLine($"[Notification] {title}: {message}");
        return Task.CompletedTask;
    }
}
