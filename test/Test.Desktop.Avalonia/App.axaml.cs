using Ddon.Desktop.Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Desktop.Avalonia.Services;

namespace Test.Desktop.Avalonia;

public partial class App : DesktopApplication
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuthService>();
        services.AddSingleton<PlcReadService>();
    }
}
