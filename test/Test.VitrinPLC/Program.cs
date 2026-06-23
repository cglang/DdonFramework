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
                c => { c.Ip = "127.0.0.1"; c.Port = 102; c.Rack = 0; c.Slot = 1; },
                h =>
                {
                    h.ScanInterval = 200;
                    h.MapTag("Temp", "DB1.DBD0", PlcDataType.Float);
                    h.MapTag("Run", "DB1.DBX10.0", PlcDataType.Bool);
                    h.MapTag("Speed", "DB1.DBW4", PlcDataType.Int16);
                    h.MapTag("Alarm", "DB1.DBX10.1", PlcDataType.Bool);
                    h.MapRegion("DB1", "DB1", 0, 512);
                });

            builder.AddSiemens("sub",
                c => { c.Ip = "127.0.0.2"; c.Port = 103; },
                h =>
                {
                    h.ScanInterval = 200;
                    h.MapTag("Temp", "DB1.DBD0", PlcDataType.Float);
                    h.MapTag("Run", "DB1.DBX10.0", PlcDataType.Bool);
                    h.MapTag("Speed", "DB1.DBW4", PlcDataType.Int16);
                    h.MapTag("Alarm", "DB1.DBX10.1", PlcDataType.Bool);
                    h.MapRegion("DB1", "DB1", 0, 512);
                });
        });

        services.AddSingleton<MyPlcService>();
    })
    .Build();

await host.StartAsync();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Demo");
var hub = host.Services.GetRequiredService<IPlcHub>();

logger.LogInformation("已注册 PLC: {Names}", string.Join(", ", hub.Names));

logger.LogInformation("═══ 读取示例 ═══");
float temp = hub.For("main").Get<float>("Temp");
bool run = hub.For("main").Get<bool>("Run");
short speed = hub.For("main").Get<short>("Speed");
logger.LogInformation("main.Temp={Temp}°C  Run={Run}  Speed={Speed}rpm", temp, run, speed);

logger.LogInformation("═══ 写入示例 ═══");
var result = await hub.For("main").SetAsync("Run", true);
logger.LogInformation("写入结果: {Result}", result);

_ = Task.Run(async () =>
{
    for (int i = 0; i < 5; i++)
    {
        await Task.WhenAll(
            hub.For("main").SetAsync("Speed", (short)(1500 + i * 100)),
            hub.For("main").SetAsync("Temp", 25.5f + i));
        await Task.Delay(1000);
    }
});

logger.LogInformation("═══ 订阅示例 ═══");
using var sub1 = hub.For("main").Subscribe<float>("Temp",
    v => logger.LogInformation("[变化] main.Temp = {V}°C", v));
using var sub2 = hub.For("main").Subscribe<bool>("Run",
    v => logger.LogInformation("[变化] main.Run = {V}", v));
using var sub3 = hub.For("main").Subscribe<bool>("Alarm", v =>
{
    if (v) logger.LogWarning("[变化] 警报触发！");
    else logger.LogInformation("[变化] 警报已清除。");
});

await Task.Delay(TimeSpan.FromSeconds(10));
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
