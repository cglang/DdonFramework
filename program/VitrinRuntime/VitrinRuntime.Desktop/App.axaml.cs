using Ddon.Desktop.Avalonia;
using Ddon.VitrinPLC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VitrinRuntime.Services;

namespace VitrinRuntime;

public partial class App : DesktopApplication
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // 配置 Serilog 日志
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
        // 注册 PLC 统一内存镜像框架（初始无预配置PLC，后续通过 Bridge 动态添加）
        services.AddVitrinPlc(builder => { });

        // 注册 PLC 配置存储（JSON 文件持久化，后续可替换为数据库实现）
        services.AddSingleton<IPlcConfigStore, JsonPlcConfigStore>();

        // 注册 Bridge Services（通过 [BridgeService] 自动发现）
        services.AddSingleton<PlcManagerService>();
        services.AddSingleton<PlcDataService>();

        // 注册后台事件推送（WebView 模式下前端可通过事件自动刷新点位数据）
        services.AddHostedService<TagChangeMonitorService>();
    }
}
