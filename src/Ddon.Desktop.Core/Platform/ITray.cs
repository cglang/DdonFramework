namespace Ddon.Desktop.Core.Platform;

public interface ITray
{
    void Show();
    void Hide();
    void SetIcon(string iconPath);
    void SetTooltip(string text);
}
