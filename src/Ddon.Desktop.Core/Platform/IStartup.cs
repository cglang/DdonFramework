namespace Ddon.Desktop.Core.Platform;

public interface IStartup
{
    bool IsEnabled { get; }
    void Enable();
    void Disable();
}
