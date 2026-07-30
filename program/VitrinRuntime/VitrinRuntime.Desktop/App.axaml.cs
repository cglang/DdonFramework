using Ddon.Desktop.Avalonia;
using Ddon.OpcUaServer;
using Ddon.VitrinPLC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VitrinRuntime.Desktop.HostedServices;
using VitrinRuntime.Desktop.Services;
using VitrinRuntime.Desktop.Stores;
using VitrinRuntime.Services;

namespace VitrinRuntime.Desktop;

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

        // 注册 EventBus（自动扫描当前程序集中的 IEventHandler 实现）
        services.AddEventBus(typeof(App).Assembly);

        // 注册 Bridge Services（通过 [BridgeService] 自动发现）
        services.AddSingleton<PlcManager>();
        services.AddSingleton<PlcDataService>();

        // 注册点位历史记录存储（JSON 文件持久化）
        services.AddSingleton<ITagHistoryStore, JsonTagHistoryStore>();

        // 注册点位变化订阅管理器（替代原来的 TagChangeMonitorService 轮询机制）
        services.AddSingleton<TagSubscriptionManager>();

        services.AddHostedService<AutoConnectHostedService>();

        // ── Lua 脚本引擎 ────────────────────────────────────────
        services.AddLua(options => { options.EnableFileWatcher = true; });
        services.AddSingleton<ILuaGroupStore, JsonLuaGroupStore>();
        services.AddSingleton<LuaEventBridgeService>();
        services.AddSingleton<LuaEngineService>();
        services.AddHostedService<LuaAutoLoadService>();

        // ── OPC UA Server ──────────────────────────────────────────
        services.AddVitrinUaServer(options =>
        {
            options.ServerName = "VitrinRuntime";
            options.EndpointUrl = "opc.tcp://localhost:4840";
            options.AllowAnonymous = true;
        });
        services.AddSingleton<OpcUaServerService>();
        services.AddHostedService<OpcUaHostedService>();
    }
}
