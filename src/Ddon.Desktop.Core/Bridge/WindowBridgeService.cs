using Ddon.Desktop.Core.Annotations;
using Ddon.Desktop.Core.Platform;

namespace Ddon.Desktop.Core.Bridge;

/// <summary>
/// 将 IWindow 的窗口能力暴露给前端,前端通过 ui.invoke("window.xxx") 调用。
/// 平台实现由各桌面项目通过 DI 注入 IWindow(AvaloniaWindow / WpfWindow)。
/// </summary>
[BridgeService(Name = "window")]
public class WindowBridgeService
{
    private readonly IWindow _window;

    public WindowBridgeService(IWindow window)
    {
        _window = window;
    }

    /// <summary>拖动无边框窗口</summary>
    [BridgeMethod]
    public void Drag() => _window.WindowDrag();

    [BridgeMethod]
    public void Minimize() => _window.Minimize();

    [BridgeMethod]
    public void Maximize() => _window.Maximize();

    [BridgeMethod]
    public void Restore() => _window.Restore();

    [BridgeMethod]
    public void Close() => _window.Close();

    [BridgeMethod(Name = "setTitle")]
    public void SetTitle(string title) => _window.SetTitle(title);
}
