namespace Ddon.Desktop.Core.Platform;

public interface INotification
{
    Task ShowAsync(string title, string message);
}
