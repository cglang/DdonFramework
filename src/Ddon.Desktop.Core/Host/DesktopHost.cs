using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ddon.Desktop.Core.Host;

public class DesktopHost
{
    private WebApplication? _app;

    public async Task<WebApplication> StartAsync(string[] args, Action<DesktopHostBuilder> configure)
    {
        var builder = new DesktopHostBuilder();
        configure(builder);

        var webBuilder = WebApplication.CreateBuilder(args);

        webBuilder.WebHost.UseUrls(builder.Urls);
        webBuilder.Logging.ClearProviders();

        webBuilder.Services.AddDesktop();
        foreach (var sa in builder.ServiceActions)
            sa(webBuilder.Services);

        webBuilder.Services.AddCors(o =>
        {
            o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });
        webBuilder.Services.AddControllers()
            .AddApplicationPart(typeof(BridgeController).Assembly)
            .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

        var app = webBuilder.Build();

        app.UseCors();
        app.UseFileServer();
        app.MapControllers();
        
        await app.StartAsync();
        _app = app;

        foreach (var ia in builder.InitActions)
            await ia(app.Services);

        return app;
    }

    public async Task StopAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
