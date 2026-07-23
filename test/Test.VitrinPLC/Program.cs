using System.Linq;
using Ddon.VitrinPLC;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices(services =>
    {
        services.AddVitrinPlc(builder =>
        {
            builder.AddSiemens("main",
                c => { c.Ip = "127.0.0.1"; c.Port = 104; c.Rack = 0; c.Slot = 1; },
                h =>
                {
                    h.ScanInterval = 200;
                    h.MapRegion("DB1", "DB1", 0, 512);
                    h.MapRegion("DB2", "DB2", 0, 256);
                    h.MapTag("Temp", "DB1.DBD0", PlcDataType.Float);
                    h.MapTag("Run", "DB1.DBX10.0", PlcDataType.Bool);
                    h.MapTag("Speed", "DB1.DBW4", PlcDataType.Int16);
                    h.MapTag("Alarm", "DB1.DBX10.1", PlcDataType.Bool);
                });

            builder.AddSiemens("sub",
                c => { c.Ip = "127.0.0.2"; c.Port = 103; },
                h =>
                {
                    h.ScanInterval = 200;
                    h.MapRegion("DB1", "DB1", 0, 512);
                    h.MapRegion("DB2", "DB2", 0, 256);
                    h.MapTag("Temp", "DB1.DBD0", PlcDataType.Float);
                    h.MapTag("Run", "DB1.DBX10.0", PlcDataType.Bool);
                    h.MapTag("Speed", "DB1.DBW4", PlcDataType.Int16);
                    h.MapTag("Alarm", "DB1.DBX10.1", PlcDataType.Bool);
                });
        });

        services.AddSingleton<MyPlcService>();
    })
    .Build();

await host.StartAsync();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Demo");
var hub = host.Services.GetRequiredService<IPlcHub>();

// ═══════════════════════════════════════════════════════
logger.LogInformation("═══════════════════════════════════════");
logger.LogInformation("  Test.VitrinPLC — 运行时 Tag 添加演示");
logger.LogInformation("═══════════════════════════════════════");

logger.LogInformation("已注册 PLC: {Names}", string.Join(", ", hub.Names));

var session = hub.For("main");
logger.LogInformation("启动 Tag: {Tags}", string.Join(", ", session.Tags.Select(t => t.Name)));

// ═══════════════════════════════════════════════════════
// 一、读取示例
// ═══════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("═══ 一、读取示例 ═══");
float temp = session.Get<float>("Temp");
bool run = session.Get<bool>("Run");
short speed = session.Get<short>("Speed");
logger.LogInformation("main.Temp={Temp}°C  Run={Run}  Speed={Speed}rpm", temp, run, speed);

// ═══════════════════════════════════════════════════════
// 二、运行时动态添加 Tag
// ═══════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("═══ 二、运行时 AddTag ═══");

var newTag = new TagDefinition("Pressure", "DB1.DBD8", PlcDataType.Float);
logger.LogInformation("→ 动态添加: {Tag}", newTag);
session.AddTag(newTag);
logger.LogInformation("  当前 Tags: {Tags}", string.Join(", ", session.Tags.Select(t => t.Name)));

// 等待一个扫描周期让同步引擎注册新区域
await Task.Delay(300);
float pressure = session.Get<float>("Pressure");
logger.LogInformation("  Pressure = {Value} (初始值)", pressure);

// ═══════════════════════════════════════════════════════
// 三、写入动态 Tag
// ═══════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("═══ 三、写入动态 Tag ═══");
var result = await session.SetAsync("Pressure", 101.3f);
logger.LogInformation("  SetAsync Pressure=101.3 → {Result}", result.Success ? "成功" : "失败");

// 等待扫描刷新
await Task.Delay(400);
pressure = session.Get<float>("Pressure");
logger.LogInformation("  刷新后 Pressure = {Value}", pressure);

// ═══════════════════════════════════════════════════════
// 四、运行时添加第二个 Tag（Bool 类型）
// ═══════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("═══ 四、运行时添加 Bool Tag ═══");
session.AddTag(new TagDefinition("ValveOpen", "DB1.DBX12.0", PlcDataType.Bool));
logger.LogInformation("  当前 Tags: {Tags}", string.Join(", ", session.Tags.Select(t => t.Name)));

await Task.Delay(300);
bool valve = session.Get<bool>("ValveOpen");
logger.LogInformation("  ValveOpen = {Value}", valve);

await session.SetAsync("ValveOpen", true);
await Task.Delay(400);
valve = session.Get<bool>("ValveOpen");
logger.LogInformation("  写入后 ValveOpen = {Value}", valve);

// ═══════════════════════════════════════════════════════
// 五、订阅动态 Tag 变化
// ═══════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("═══ 五、订阅动态 Tag 变化 ═══");
using var subPressure = session.Subscribe<float>("Pressure",
    (oldV, newV) => logger.LogInformation("[变化] Pressure: {Old} → {New} hPa", oldV, newV));
using var subValve = session.Subscribe<bool>("ValveOpen",
    (oldV, newV) => logger.LogInformation("[变化] ValveOpen: {Old} → {New}", oldV, newV));

// 后台持续写入，触发变化检测
_ = Task.Run(async () =>
{
    for (int i = 0; i < 5; i++)
    {
        await Task.WhenAll(
            session.SetAsync("Speed", (short)(1500 + i * 100)),
            session.SetAsync("Temp", 25.5f + i),
            session.SetAsync("Pressure", 101.3f + i * 0.5f));
        await Task.Delay(1000);
    }
});

// 也订阅原有 Tag
using var subTemp = session.Subscribe<float>("Temp",
    (oldV, newV) => logger.LogInformation("[变化] Temp: {Old} → {New}°C", oldV, newV));
using var subRun = session.Subscribe<bool>("Run",
    (oldV, newV) => logger.LogInformation("[变化] Run: {Old} → {New}", oldV, newV));
using var subAlarm = session.Subscribe<bool>("Alarm", v =>
{
    if (v) logger.LogWarning("[变化] 警报触发！");
    else logger.LogInformation("[变化] 警报已清除。");
});

// ═══════════════════════════════════════════════════════
// 六、运行时动态添加 PLC
// ═══════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("═══ 六、运行时 AddPlcAsync ═══");
logger.LogInformation("→ 动态添加 PLC: 'line2' (请修改为真实 PLC IP)");

try
{
    await hub.AddPlcAsync("line2",
        new Ddon.VitrinPLC.Clients.SiemensClient(
            new Ddon.VitrinPLC.Clients.SiemensOptions
            {
                Name = "line2", Ip = "127.0.0.3", Port = 105, Rack = 0, Slot = 1
            },
            host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Ddon.VitrinPLC.Clients.SiemensClient>()),
        h =>
        {
            h.ScanInterval = 200;
            h.MapRegion("DB1", "DB1", 0, 512);
            h.MapTag("LineSpeed", "DB1.DBW0", PlcDataType.Int16);
        });

    logger.LogInformation("  当前所有 PLC: {Names}", string.Join(", ", hub.Names));

    var line2 = hub.For("line2");
    logger.LogInformation("  line2 Tags: {Tags}", string.Join(", ", line2.Tags.Select(t => t.Name)));

    // 为新 PLC 运行时添加 Tag
    line2.AddTag(new TagDefinition("LineTemp", "DB1.DBD4", PlcDataType.Float));
    logger.LogInformation("  line2 Tags (添加后): {Tags}", string.Join(", ", line2.Tags.Select(t => t.Name)));

    await Task.Delay(400);
    short lineSpeed = line2.Get<short>("LineSpeed");
    float lineTemp = line2.Get<float>("LineTemp");
    logger.LogInformation("  LineSpeed={Speed}  LineTemp={Temp}°C", lineSpeed, lineTemp);
}
catch (Exception ex)
{
    logger.LogWarning(ex, "  添加 PLC 'line2' 失败（无真实 PLC），跳过此演示");
    // 清理 AddPlcAsync 失败时可能残留的注册
    try { await hub.RemovePlcAsync("line2"); } catch { }
}

// ═══════════════════════════════════════════════════════
// 七、运行 8 秒观察变化
// ═══════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("═══ 七、运行 8 秒，观察订阅输出 ═══");
await Task.Delay(TimeSpan.FromSeconds(8));

// 展示最终快照
logger.LogInformation("");
logger.LogInformation("═══ 最终状态 ═══");
logger.LogInformation("main:  Temp={0}  Run={1}  Speed={2}  Pressure={3}  ValveOpen={4}",
    session.Get<float>("Temp"),
    session.Get<bool>("Run"),
    session.Get<short>("Speed"),
    session.Get<float>("Pressure"),
    session.Get<bool>("ValveOpen"));

if (hub.Names.Contains("line2"))
{
    var line2Final = hub.For("line2");
    logger.LogInformation("line2: LineSpeed={0}  LineTemp={1}",
        line2Final.Get<short>("LineSpeed"),
        line2Final.Get<float>("LineTemp"));

    // ═══════════════════════════════════════════════════════
    // 八、运行时移除 PLC
    // ═══════════════════════════════════════════════════════
    logger.LogInformation("");
    logger.LogInformation("═══ 八、运行时 RemovePlcAsync ═══");
    logger.LogInformation("→ 移除 PLC: 'line2'");
    await hub.RemovePlcAsync("line2");
    logger.LogInformation("  剩余 PLC: {Names}", string.Join(", ", hub.Names));
}

logger.LogInformation("");
logger.LogInformation("正在停止...");
await host.StopAsync();

public sealed class MyPlcService(IPlcHub hub, ILogger<MyPlcService> logger)
{
    public void LogSnapshot()
    {
        float temp = hub.For("main").Get<float>("Temp");
        bool run = hub.For("main").Get<bool>("Run");
        short speed = hub.For("main").Get<short>("Speed");
        logger.LogInformation("Temp={Temp}°C  Run={Run}  Speed={Speed}rpm", temp, run, speed);
    }

    public async Task EmergencyStopAsync(CancellationToken ct = default)
    {
        var r1 = await hub.For("main").SetAsync("Run", false, ct);
        var r2 = await hub.For("main").SetAsync("Speed", (short)0, ct);
        logger.LogWarning("紧急停止: Run={R1} Speed={R2}", r1.Success, r2.Success);
    }
}
