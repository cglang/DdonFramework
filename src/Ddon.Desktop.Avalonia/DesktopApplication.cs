using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Ddon.Desktop.Avalonia.Platform;
using Ddon.Desktop.Avalonia.Transport;
using Ddon.Desktop.Core.Bridge;
using Ddon.Desktop.Core.Host;
using Ddon.Desktop.Core.Platform;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Desktop.Avalonia;

public abstract class DesktopApplication : Application
{
    private DesktopHost? _desktopHost;

    private IConfiguration Configuration { get; set; } = null!;

    protected abstract void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    protected virtual IConfigurationBuilder CreateConfigurationBuilder()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: false);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Startup += OnDesktopStartup;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void OnDesktopStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        Configuration = CreateConfigurationBuilder().Build();

        var backendUrls = Configuration.GetValue<string>("HostUrls:BackendUrl")
                          ?? throw new Exception("未配置 HostUrls:BackendUrl");

        var navigateUrl = Configuration.GetValue<string>("HostUrls:FrontendUrl")
                          ?? throw new Exception("未配置 HostUrls:FrontendUrl");

        var windowTitle = Configuration.GetValue<string>("Window:Title") ?? "Ddon Desktop";
        var loadingTitle = Configuration.GetValue<string>("Window:LoadingTitle") ?? "Ddon Desktop";
        var loadingText = Configuration.GetValue<string>("Window:LoadingText") ?? "正在启动服务...";

        // Normal、Maximized、Minimized、FullScreen
        var windowState = Configuration.GetValue<string>("Window:WindowState") ?? "Maximized";
        var windowWidth = Configuration.GetValue<int>("Window:Width");
        var windowHeight = Configuration.GetValue<int>("Window:Height");

        var mainWindow = new MainWindow()
        {
            WindowDecorations = WindowDecorations.None
        };

        // 这里需要启动一个桌面窗口 用来加载WebView
        mainWindow.ApplyConfig(windowTitle, loadingTitle, loadingText,
            Enum.TryParse<WindowState>(windowState, true, out var ws)
                ? ws
                : WindowState.Maximized,
            windowWidth > 0 ? windowWidth : 800,
            windowHeight > 0 ? windowHeight : 500);
        mainWindow.SetOnClosing(() => _desktopHost?.StopAsync() ?? Task.CompletedTask);
        mainWindow.Show();

        // 窗体启动后，启动桌面服务
        _desktopHost = new DesktopHost();
        await _desktopHost.StartAsync(e.Args, builder =>
        {
            builder.UseUrls(backendUrls);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(Configuration);
                services.AddDesktop();
                // 窗口能力注册为桥接服务,前端通过 ui.invoke("window.xxx") 调用
                services.AddSingleton<IWindow>(_ => new AvaloniaWindow(mainWindow));
                services.AddSingleton<WindowBridgeService>();
                services.AddDesktopTransport<AvaloniaWebViewTransport>();
                ConfigureServices(services, Configuration);
            });
            builder.OnInitialized(async services =>
            {
                var transport = services.GetRequiredService<AvaloniaWebViewTransport>();
                await mainWindow.InitializeWebViewAsync(navigateUrl, transport);

                // 在这里设置一个延时, 这个无关紧要, 如果 WebView 加载慢了就显示这个
                // WebView 加载快了这个就无效
                await Task.Delay(500);
                mainWindow.SetLoadingText("正在准备浏览器引擎...");
            });
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.MainWindow = mainWindow;
            lifetime.ShutdownRequested += async (_, _) =>
            {
                if (_desktopHost is not null)
                    await _desktopHost.StopAsync();
            };
        }
    }
}
