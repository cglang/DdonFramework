using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Core;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.IOInterface;
using OpenProtocolInterpreter.Job;
using OpenProtocolInterpreter.ParameterSet;
using OpenProtocolInterpreter.Tightening;
using OpenProtocolInterpreter.Tool;

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 ? int.Parse(args[1]) : 4545;

Console.WriteLine("=== DdonGardener OpenProtocol 测试 ===");
Console.WriteLine($"目标: {host}:{port}");
Console.WriteLine();
Console.WriteLine("模式选择:");
Console.WriteLine("  1 - 测试模式 (订阅拧紧结果)");
Console.WriteLine("  2 - 工具模式 (工具控制)");
Console.Write("请选择 (1/2): ");
var mode = Console.ReadLine()?.Trim();

var services = new ServiceCollection();

services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

services.AddSingleton<ISocketFactory>(_ => new SocketFactory());
services.AddSingleton<IOpenProtocolManager>(sp =>
{
    var socketFactory = sp.GetRequiredService<ISocketFactory>();
    var loggerFactory = sp.GetService<ILoggerFactory>();
    var manager = new OpenProtocolManager(socketFactory, sp, loggerFactory);

    manager.AddEndpoint("扭紧机", endpoint =>
    {
        endpoint.Configure(o =>
        {
            o.Host = host;
            o.Port = port;
            o.KeepAliveIntervalMs = 10_000;
            o.RequestTimeoutMs = 5_000;
            o.AutoReconnect = true;
        });

        endpoint.MapResponse<Mid0060, Mid0061>();
        endpoint.MapResponse<Mid0062, Mid0005>();
        endpoint.MapResponse<Mid0001, Mid0002>();
        endpoint.MapResponse<Mid0003, Mid0005>();
        endpoint.MapResponse<Mid0018, Mid0005>();
        endpoint.MapResponse<Mid0042, Mid0005>();
        endpoint.MapResponse<Mid0043, Mid0005>();
        endpoint.MapResponse<Mid0037, Mid0038>();
        endpoint.MapResponse<Mid0224, Mid0005>();
    });

    return manager;
});

var sp = services.BuildServiceProvider();
var manager = sp.GetRequiredService<IOpenProtocolManager>();
var endpoint = manager.GetEndpoint("扭紧机")!;

endpoint.SubscribeAll(mid =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ← MID{mid.Header.Mid:D4}");
    return Task.CompletedTask;
});

Console.WriteLine($"正在连接 {host}:{port}...");
await endpoint.StartAsync();
Console.WriteLine("连接成功.");

if (mode == "2")
{
    await RunToolMode(endpoint);
}
else
{
    await RunTestMode(endpoint);
}

await endpoint.StopAsync();
Console.WriteLine("已断开连接.");

static async Task RunTestMode(IOpenProtocolEndpoint endpoint)
{
    endpoint.Subscribe<Mid0061>(result =>
    {
        var status = result.TighteningStatus ? "OK" : "NOK";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] 拧紧结果 | " +
            $"ID={result.TighteningId} " +
            $"扭矩={result.Torque:F2}Nm " +
            $"角度={result.Angle:F0}度 " +
            $"状态={status} " +
            $"VIN={result.VinNumber}");
        Console.ResetColor();
    });

    Console.WriteLine("正在订阅拧紧结果 (MID0060)...");
    var firstResult = await endpoint.SubscribeAsync<Mid0061>(new Mid0060());
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(
        $"[首个结果] 扭矩={firstResult.Torque:F2}Nm " +
        $"角度={firstResult.Angle:F0}度 " +
        $"状态={(firstResult.TighteningStatus ? "OK" : "NOK")}");
    Console.ResetColor();

    Console.WriteLine();
    Console.WriteLine("=== 测试模式 ===");
    Console.WriteLine("命令:");
    Console.WriteLine("  status         - 查看连接状态");
    Console.WriteLine("  subscribe      - 重新订阅拧紧结果");
    Console.WriteLine("  select <编号>  - 选择参数组 (Mid0018)");
    Console.WriteLine("  exit           - 断开并退出");
    Console.WriteLine();

    while (true)
    {
        Console.Write("> ");
        var input = Console.ReadLine();
        if (input is null || input == "exit") break;

        try
        {
            var parts = input.Split(' ', 2);
            var cmd = parts[0].ToLowerInvariant();

            switch (cmd)
            {
                case "status":
                    Console.WriteLine($"状态: {endpoint.State}");
                    break;

                case "subscribe":
                    await endpoint.RegisterSubscriptionAsync(new Mid0060());
                    Console.WriteLine("已订阅 MID0060.");
                    break;

                case "select":
                    if (parts.Length > 1 && int.TryParse(parts[1], out var pset))
                    {
                        var mid0018 = new Mid0018 { ParameterSetId = pset };
                        await endpoint.SendAsync<Mid0005>(mid0018);
                        Console.WriteLine($"已选择参数组 {pset}.");
                    }
                    else
                    {
                        Console.WriteLine("用法: select <参数组编号>");
                    }
                    break;

                default:
                    Console.WriteLine($"未知命令: {cmd}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"错误: {ex.Message}");
            Console.ResetColor();
        }
    }
}

static async Task RunToolMode(IOpenProtocolEndpoint endpoint)
{
    Console.WriteLine();
    Console.WriteLine("=== 工具模式 ===");
    Console.WriteLine("命令:");
    Console.WriteLine("  status         - 查看连接状态");
    Console.WriteLine("  disable        - 工具断开 (MID0042)");
    Console.WriteLine("  enable         - 工具使能 (MID0043)");
    Console.WriteLine("  start          - 启动工具 (MID0037)");
    Console.WriteLine("  select <编号>  - 选择程序号 (Mid0018)");
    Console.WriteLine("  exit           - 断开并退出");
    Console.WriteLine();

    while (true)
    {
        Console.Write("工具> ");
        var input = Console.ReadLine();
        if (input is null || input == "exit") break;

        try
        {
            var parts = input.Split(' ', 2);
            var cmd = parts[0].ToLowerInvariant();

            switch (cmd)
            {
                case "status":
                    Console.WriteLine($"状态: {endpoint.State}");
                    break;

                case "disable":
                    await endpoint.SendAsync<Mid0005>(new Mid0042());
                    Console.WriteLine("工具已断开.");
                    break;

                case "enable":
                    await endpoint.SendAsync<Mid0005>(new Mid0043());
                    Console.WriteLine("工具已使能.");
                    break;

                case "start":
                    await endpoint.SendAsync<Mid0005>(new Mid0224());
                    Console.WriteLine("工具已启动.");
                    break;

                case "select":
                    if (parts.Length > 1 && int.TryParse(parts[1], out var pset))
                    {
                        var mid0018 = new Mid0018 { ParameterSetId = pset };
                        await endpoint.SendAsync<Mid0005>(mid0018);
                        Console.WriteLine($"已选择程序号 {pset}.");
                    }
                    else
                    {
                        Console.WriteLine("用法: select <程序编号>");
                    }
                    break;

                default:
                    Console.WriteLine($"未知命令: {cmd}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"错误: {ex.Message}");
            Console.ResetColor();
        }
    }
}
