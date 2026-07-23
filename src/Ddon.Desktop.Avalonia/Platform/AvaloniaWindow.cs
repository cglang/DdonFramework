using Avalonia.Controls;
using Ddon.Desktop.Core.Platform;

namespace Ddon.Desktop.Avalonia.Platform;

public class AvaloniaWindow : IWindow
{
    private readonly Window _window;

    public AvaloniaWindow(Window window)
    {
        _window = window;
    }

    public void Minimize() => _window.WindowState = WindowState.Minimized;
    public void Maximize() => _window.WindowState = WindowState.Maximized;
    public void Restore() => _window.WindowState = WindowState.Normal;
    public void Close() => _window.Close();
    public void SetTitle(string title) => _window.Title = title;
}
