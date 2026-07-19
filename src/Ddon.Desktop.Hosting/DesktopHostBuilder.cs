using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Desktop.Hosting;

public class DesktopHostBuilder
{
    public string Urls { get; set; } = "http://localhost:5000";

    internal List<Action<IServiceCollection>> ServiceActions { get; } = new();
    internal List<Func<IServiceProvider, Task>> InitActions { get; } = new();

    public DesktopHostBuilder UseUrls(string urls)
    {
        Urls = urls;
        return this;
    }

    public DesktopHostBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        ServiceActions.Add(configure);
        return this;
    }

    public DesktopHostBuilder OnInitialized(Func<IServiceProvider, Task> action)
    {
        InitActions.Add(action);
        return this;
    }
}
