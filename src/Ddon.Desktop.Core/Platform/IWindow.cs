namespace Ddon.Desktop.Platform;

public interface IWindow
{
    void Minimize();
    void Maximize();
    void Restore();
    void Close();
    void SetTitle(string title);
}
