using Ddon.VitrinPLC;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Ddon.VitrinPLC.TagEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ══════════════════════════════════════════════════════════════
//  PLC 统一内存镜像架构 — 完整使用示例
//  演示：启动框架 → 读值 → 写值 → 订阅变化 → 优雅停止
// ══════════════════════════════════════════════════════════════

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    })
    .ConfigureServices(services =>
    {
        // ── 注册 PLC 镜像框架（一行搞定）────────────────────
        services.AddPlcMirror(x =>
        {
            // 选择协议（切换只需换一行）
            x.UseSiemens("Main-PLC", plc =>
            {
                plc.Ip = "127.0.0.1";
                plc.Port = 102;
                plc.Rack = 0;
                plc.Slot = 1;
            });

            // 扫描周期（毫秒）
            x.ScanInterval = 200;

            // 映射 Tag（名称、地址、类型）
            x.MapTag("Temp", "DB1.DBD0", PlcDataType.Float);
            x.MapTag("Run", "DB1.DBX10.0", PlcDataType.Bool);
            x.MapTag("Speed", "DB1.DBW4", PlcDataType.Int16);
            x.MapTag("Count", "DB1.DBW4", PlcDataType.Int16);
            x.MapTag("Alarm", "DB1.DBX10.1", PlcDataType.Bool);
            x.MapTag("Name", "DB2.DBB0", PlcDataType.String, stringLength: 20);

            // 可选：手动指定内存区域大小（不配置则自动推断 4096 bytes）
            x.MapRegion("DB1", "DB1", 0, 512);
            x.MapRegion("DB2", "DB2", 0, 256);
            //x.MapRegion("M", "M", 0, 2048);
        });

        // 自己的业务服务也可以正常注册
        services.AddSingleton<MyDashboardService>();
    })
    .Build();

// ─────────────────────────────────────────────
// 启动 Host（PlcMirrorHostedService 自动启动同步引擎）
// ─────────────────────────────────────────────
await host.StartAsync();

var tags = host.Services.GetRequiredService<ITagService>();
var engine = host.Services.GetRequiredService<IPlcSyncEngine>();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Demo");

// ─────────────────────────────────────────────
// 示例1：读取镜像值（同步，无 IO，极低延迟）
// ─────────────────────────────────────────────
logger.LogInformation("═══ 读取示例 ═══");
float temp = tags.Get<float>("Temp");
bool run = tags.Get<bool>("Run");
short speed = tags.Get<short>("Speed");
logger.LogInformation("Temp={Temp}°C  Run={Run}  Speed={Speed}rpm", temp, run, speed);

// ─────────────────────────────────────────────
// 示例2：写入 PLC（直接写，不等扫描，返回结果对象）
// ─────────────────────────────────────────────
logger.LogInformation("═══ 写入示例 ═══");
var result = await tags.SetAsync("Run", true);
logger.LogInformation("{Result}", result);

// 批量写入（并发）
_ = Task.Run(async () =>
{
    for (int i = 0; i < 10; i++)
    {
        var t1 = tags.SetAsync("Speed", (short)1500 + i);
        var t2 = tags.SetAsync("Temp", 25.5f + i);       // 写 Float
        await Task.WhenAll(t1, t2);
        await Task.Delay(1000);
    }
});



// ─────────────────────────────────────────────
// 示例3：订阅变化（在扫描后触发）
// ─────────────────────────────────────────────
logger.LogInformation("═══ 订阅示例 ═══");
using var sub1 = ((TagService)tags).Subscribe<float>("Temp", v =>
    logger.LogInformation("[变化通知] Temp = {V}°C", v));

using var sub2 = ((TagService)tags).Subscribe<bool>("Run", v =>
    logger.LogInformation("[变化通知] Run  = {V}", v));

using var sub3 = ((TagService)tags).Subscribe<bool>("Alarm", v =>
{
    if (v) logger.LogWarning("⚠️  警报触发！");
    else logger.LogInformation("✅  警报已清除。");
});

// ─────────────────────────────────────────────
// 示例4：监听扫描完成事件
// ─────────────────────────────────────────────
engine.ScanCompleted += (_, e) =>
{
    if (e.HasChanges)
        logger.LogInformation("扫描完成 v{Version} | 耗时 {Elapsed:F1}ms | {Count} 个变化",
            e.Version, e.Elapsed.TotalMilliseconds, e.Changes.Count);
};

// ─────────────────────────────────────────────
// 示例5：模拟手动单次扫描（可用于测试）
// ─────────────────────────────────────────────
await engine.ScanOnceAsync();

// ─────────────────────────────────────────────
// 运行 10 秒后优雅停止
// ─────────────────────────────────────────────
await Task.Delay(TimeSpan.FromSeconds(10));
logger.LogInformation("正在停止...");
await host.StopAsync();

// ══════════════════════════════════════════════════════════════
//  业务服务示例（注入 ITagService 使用）
// ══════════════════════════════════════════════════════════════
public sealed class MyDashboardService
{
    private readonly ITagService _tags;
    private readonly ILogger<MyDashboardService> _logger;

    public MyDashboardService(ITagService tags, ILogger<MyDashboardService> logger)
    {
        _tags = tags;
        _logger = logger;
    }

    public async Task<DashboardSnapshot> GetSnapshotAsync()
    {
        // 所有读取都来自内存镜像，不产生任何 PLC 通信
        return new DashboardSnapshot
        {
            Temperature = _tags.Get<float>("Temp"),
            IsRunning = _tags.Get<bool>("Run"),
            Speed = _tags.Get<short>("Speed"),
            Count = _tags.Get<short>("Count"),
            Alarm = _tags.Get<bool>("Alarm"),
            Timestamp = DateTime.Now
        };
    }

    public async Task EmergencyStopAsync(CancellationToken ct = default)
    {
        var r1 = await _tags.SetAsync("Run", false, ct);
        var r2 = await _tags.SetAsync("Speed", (short)0, ct);

        _logger.LogWarning("紧急停止已发送: Run={R1}, Speed={R2}", r1.Success, r2.Success);
        // 注意：设计原则4 ─ 写入已发送，结果在下次扫描后才反映到镜像
    }
}

public sealed record DashboardSnapshot
{
    public float Temperature { get; init; }
    public bool IsRunning { get; init; }
    public short Speed { get; init; }
    public short Count { get; init; }
    public bool Alarm { get; init; }
    public DateTime Timestamp { get; init; }
}
