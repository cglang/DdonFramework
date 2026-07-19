namespace Ddon.Desktop.Platform;

public interface INotification
{
    Task ShowAsync(string title, string message);
}
