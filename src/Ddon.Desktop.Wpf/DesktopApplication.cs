using System.Windows;
using Ddon.Desktop.Core.Bridge;
using Ddon.Desktop.Core.Host;
using Ddon.Desktop.Core.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Desktop.Wpf;

public abstract class DesktopApplication : Application
{
    private DesktopHost? _desktopHost;
    private MainWindow? _mainWindow;

    protected IConfiguration Configuration { get; private set; } = null!;

    protected abstract void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    protected virtual IConfigurationBuilder CreateConfigurationBuilder()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: false);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Configuration = CreateConfigurationBuilder().Build();

        var backendUrls = Configuration.GetValue<string>("HostUrls:BackendUrl")
            ?? throw new Exception("未配置 HostUrls:BackendUrl");

        var navigateUrl = Configuration.GetValue<string>("HostUrls:FrontendUrl")
            ?? throw new Exception("未配置 HostUrls:FrontendUrl");

        var windowTitle = Configuration.GetValue<string>("Window:Title") ?? "Ddon Desktop";
        var loadingTitle = Configuration.GetValue<string>("Window:LoadingTitle") ?? "Ddon Desktop";
        var loadingText = Configuration.GetValue<string>("Window:LoadingText") ?? "正在启动服务...";

        _mainWindow = new MainWindow();
        _mainWindow.ApplyConfig(windowTitle, loadingTitle, loadingText);
        _mainWindow.Show();

        _desktopHost = new DesktopHost();
        await _desktopHost.StartAsync(e.Args, builder =>
        {
            builder.UseUrls(backendUrls);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(Configuration);
                services.AddDesktop();
                services.AddDesktopTransport<WebViewTransport>();
                ConfigureServices(services, Configuration);
            });
            builder.OnInitialized(async sp =>
            {
                _mainWindow!.SetLoadingText("正在准备浏览器引擎...");
                _mainWindow!.BridgeDispatcher = sp.GetRequiredService<IBridgeDispatcher>();
                await _mainWindow.InitializeWebViewAsync(navigateUrl);
            });
        });
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_desktopHost is not null)
            await _desktopHost.StopAsync();
        base.OnExit(e);
    }
}
