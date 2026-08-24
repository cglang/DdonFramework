using System.Windows;
using System.Windows.Input;
using Ddon.Desktop.Core.Platform;

namespace Ddon.Desktop.Wpf.Platform;

public class WpfWindow : IWindow
{
    private readonly Window _window;

    public WpfWindow(Window window)
    {
        _window = window;
    }

    public void Minimize() => _window.WindowState = WindowState.Minimized;
    public void Maximize() => _window.WindowState = WindowState.Maximized;
    public void Restore() => _window.WindowState = WindowState.Normal;
    public void Close() => _window.Close();
    public void SetTitle(string title) => _window.Title = title;

    public void WindowDrag()
    {
        // DragMove 仅在鼠标左键按下时有效
        if (Mouse.LeftButton == MouseButtonState.Pressed)
            _window.DragMove();
    }
}
