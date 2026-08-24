using Avalonia.Controls;
using Ddon.Desktop.Core.Platform;
using System.Runtime.InteropServices;

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


#if WINDOWS
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private static readonly IntPtr HTCAPTION = new(2);
#endif

    public void WindowDrag()
    {
#if WINDOWS
        var handle = _window.TryGetPlatformHandle();
        if (handle == null)
            return;

        var hwnd = handle.Handle;

        ReleaseCapture();
        PostMessage(hwnd, WM_NCLBUTTONDOWN, HTCAPTION, IntPtr.Zero);
#else
        // 非 Windows 平台什么也不做
#endif
    }
}
