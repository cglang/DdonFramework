using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter.Alarm;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.KeepAlive;
using OpenProtocolInterpreter.Tightening;
using OpenProtocolInterpreter.Tool;

// ============================================================================
// Test.OpenProtocol —— Ddon.OpenProtocol 简化版客户端功能测试控制台
//
// 用法: Test.OpenProtocol [host] [port]
//   默认 host=127.0.0.1, port=4545
//
// 模型: 严格的「发-收-发-收」单线程协议。
//   连接时自动完成 MID0001 -> MID0002 握手;
//   SendAsync(mid) 发送一个请求并返回下一个收到的 Mid 根类;
//   SubscribeAsync<TMid>(订阅请求, handler) 发送订阅请求等确认响应,
//     之后服务端推送的 TMid 会执行 handler（不作为普通响应）。
// ============================================================================

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 && int.TryParse(args[1], out var parsedPort) ? parsedPort : 4545;

var services = new ServiceCollection();

services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

services.AddOpenProtocol(builder =>
{
    builder.AddEndpoint("openprotocol", endpoint =>
    {
        endpoint.Configure(options =>
        {
            options.Host = host;
            options.Port = port;
            options.ConnectTimeoutMs = 5_000;
            options.RequestTimeoutMs = 10_000;
            options.KeepAliveIntervalMs = 10_000;
            options.AutoReconnect = true;
        });
    });
});

await using var sp = services.BuildServiceProvider();

var manager = sp.GetRequiredService<IOpenProtocolManager>();
var endpoint = manager.GetEndpoint("openprotocol")
    ?? throw new InvalidOperationException("未找到名为 'openprotocol' 的 endpoint。");

// ---------- 连接（内部自动完成 MID0001 -> MID0002 握手） ----------
Console.WriteLine($"正在连接 {host}:{port} ...");
await endpoint.ConnectAsync();
Console.WriteLine($"已连接。状态: {endpoint.State}");
Console.WriteLine();

// 预先注册报警订阅 handler（MID0071，未发送订阅请求，仅注册）
IDisposable? alarmSubscription = null;

PrintMenu();

while (true)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (line is null)
        break;

    var input = line.Trim();
    if (input.Length == 0)
        continue;

    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var cmd = parts[0].ToLowerInvariant();

    try
    {
        switch (cmd)
        {
            case "help":
                PrintMenu();
                break;

            case "state":
                Console.WriteLine($"状态: {endpoint.State}, IsConnected: {endpoint.IsConnected}");
                break;

            case "tool":
                var toolNumber = parts.Length > 1 && int.TryParse(parts[1], out var n) ? n : 1;
                var toolReply = await endpoint.SendAsync(new Mid0040 { ToolNumber = toolNumber });
                Console.WriteLine($"收到响应: {toolReply.GetType().Name} (MID{toolReply.Header.Mid:D4})");
                if (toolReply is Mid0041 tool)
                {
                    Console.WriteLine($"  工具编号: {tool.ToolNumber}, 序列号: {tool.ToolSerialNumber}");
                }
                break;

            case "sub":
                // 订阅拧紧结果: 发送 MID0060 订阅请求, 收到 MID0005 确认后执行自定义 ackHandler,
                // 之后服务端推送的 MID0061 交给 handler, 不会占用普通响应
                var ack = await endpoint.SubscribeAsync<Mid0061, Mid0062>(
                    new Mid0060(),
                    m =>
                    {
                        PrintTighteningResult(m);
                        return Task.CompletedTask;
                    },
                    () =>
                    {
                        Console.WriteLine("==");
                        return new Mid0062();
                    });
                Console.WriteLine($"订阅确认返回: {ack.GetType().Name} (MID{ack.Header.Mid:D4})");
                break;

            case "unsub":
                var res0063 = await endpoint.SendAsync(new Mid0063());
                Console.WriteLine($"收到响应: {res0063.GetType().Name} (MID{res0063.Header.Mid:D4})");
                break;

            case "alarm":
                // 订阅报警: 发送 MID0070 订阅请求 -> 等待确认(请求-响应);
                // 之后服务端推送 MID0071 -> 执行 handler, 并自动回 MID0072 确认(ackHandler 自定义)
                if (alarmSubscription == null)
                {
                    var alarmAck = await endpoint.SubscribeAsync<Mid0071, Mid0072>(
                        new Mid0070(),
                        m =>
                        {
                            PrintAlarm(m);
                            return Task.CompletedTask;
                        },
                        () =>
                        {
                            Console.WriteLine("  [ackHandler] 生成报警确认 MID0072 回复服务端");
                            return new Mid0072();
                        });
                    Console.WriteLine($"报警订阅请求确认: {alarmAck.GetType().Name} (MID{alarmAck.Header.Mid:D4})");
                }
                else
                {
                    Console.WriteLine("报警订阅已存在。");
                }
                break;

            case "keepalive":
                Console.WriteLine("发送心跳 (MID9999) ...");
                var keepAliveReply = await endpoint.SendAsync(new Mid9999());
                Console.WriteLine($"收到响应: {keepAliveReply.GetType().Name} (MID{keepAliveReply.Header.Mid:D4})");
                break;

            case "exit":
            case "quit":
                goto exit;

            default:
                Console.WriteLine($"未知命令: {cmd}。输入 help 查看帮助。");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[错误] {ex.GetType().Name}: {ex.Message}");
    }
}

exit:

// ---------- 清理 ----------
Console.WriteLine("正在断开连接 ...");

try
{
    await endpoint.DisconnectAsync();
}
catch
{
}

await manager.StopAllAsync();
Console.WriteLine("已退出。");

// ============================================================================
// 本地辅助函数
// ============================================================================

void PrintMenu()
{
    Console.WriteLine("可用命令:");
    Console.WriteLine("  state        查看连接状态");
    Console.WriteLine("  tool [编号]  请求工具数据 (MID0040，默认编号 1)");
    Console.WriteLine("  sub          订阅拧紧结果 (MID0060 订阅请求 -> 确认, 之后 MID0061 推送)");
    Console.WriteLine("  unsub        取消订阅拧紧结果 (MID0063 订阅请求 -> 确认)");
    Console.WriteLine("  alarm        订阅报警 (MID0070 订阅请求 -> 确认, 之后 MID0071 推送)");
    Console.WriteLine("  keepalive    发送心跳 (MID9999，等待响应)");
    Console.WriteLine("  help         显示帮助");
    Console.WriteLine("  exit         断开并退出");
    Console.WriteLine();
}

void PrintTighteningResult(Mid0061 m)
{
    Console.WriteLine();
    Console.WriteLine("=== 拧紧结果推送 (MID0061) ===");
    Console.WriteLine($"  拧紧 ID       : {m.TighteningId}");
    Console.WriteLine($"  时间          : {m.Timestamp:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  VIN           : {m.VinNumber}");
    Console.WriteLine($"  任务/参数组   : Job {m.JobId} / PSet {m.ParameterSetId} (批次 {m.BatchCounter}/{m.BatchSize})");
    Console.WriteLine($"  拧紧状态      : {(m.TighteningStatus ? "OK" : "NOK")}");
    Console.WriteLine($"  扭矩          : {m.Torque} ({m.TorqueStatus})，范围 {m.TorqueMinLimit} ~ {m.TorqueMaxLimit}");
    Console.WriteLine($"  角度          : {m.Angle} ({m.AngleStatus})，范围 {m.AngleMinLimit} ~ {m.AngleMaxLimit}");
    Console.WriteLine("=================================");
    Console.WriteLine();
}

void PrintAlarm(Mid0071 m)
{
    Console.WriteLine();
    Console.WriteLine($"=== 报警推送 (MID0071) === 代码: {m.ErrorCode}  时间: {m.Time:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  控制器就绪: {m.ControllerReadyStatus}  工具就绪: {m.ToolReadyStatus}");
    Console.WriteLine($"  报警内容  : {m.AlarmText}");
    Console.WriteLine("=================================");
    Console.WriteLine();
}
