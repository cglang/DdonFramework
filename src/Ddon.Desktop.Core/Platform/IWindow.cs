namespace Ddon.Desktop.Core.Platform;

public interface IWindow
{
    void Minimize();
    void Maximize();
    void Restore();
    void Close();
    void SetTitle(string title);
    void WindowDrag();
}
