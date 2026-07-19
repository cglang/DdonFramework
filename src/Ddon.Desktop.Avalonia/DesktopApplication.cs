using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Ddon.Desktop.Bridge;
using Ddon.Desktop.Hosting;
using Ddon.Desktop.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Desktop.Avalonia;

public abstract class DesktopApplication : Application
{
    private DesktopHost? _desktopHost;
    private MainWindow? _mainWindow;

    protected IConfiguration Configuration { get; private set; } = null!;

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

        _mainWindow = new MainWindow();
        _mainWindow.ApplyConfig(windowTitle, loadingTitle, loadingText,
            Enum.TryParse<global::Avalonia.Controls.WindowState>(windowState, true, out var ws) ? ws : global::Avalonia.Controls.WindowState.Maximized,
            windowWidth > 0 ? windowWidth : 800,
            windowHeight > 0 ? windowHeight : 500);
        _mainWindow.SetOnClosing(() => _desktopHost?.StopAsync() ?? Task.CompletedTask);
        _mainWindow.Show();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.MainWindow = _mainWindow;
        }

        _desktopHost = new DesktopHost();
        await _desktopHost.StartAsync(e.Args ?? [], builder =>
        {
            builder.UseUrls(backendUrls);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(Configuration);
                services.AddDesktop();
                services.AddDesktopTransport<AvaloniaWebViewTransport>();
                ConfigureServices(services, Configuration);
            });
            builder.OnInitialized(async sp =>
            {
                _mainWindow!.SetLoadingText("正在准备浏览器引擎...");
                _mainWindow!.BridgeDispatcher = sp.GetRequiredService<IBridgeDispatcher>();
                await _mainWindow.InitializeWebViewAsync(navigateUrl);
            });
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime2)
        {
            lifetime2.ShutdownRequested += async (s, args) =>
            {
                if (_desktopHost is not null)
                    await _desktopHost.StopAsync();
            };
        }
    }
}
