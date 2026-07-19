namespace Ddon.Desktop.Platform;

public interface IStartup
{
    bool IsEnabled { get; }
    void Enable();
    void Disable();
}
