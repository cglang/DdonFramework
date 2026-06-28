using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Builder;
using Ddon.OpenProtocol.Core;
using Ddon.OpenProtocol.Extensions;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.KeepAlive;
using OpenProtocolInterpreter.ParameterSet;
using OpenProtocolInterpreter.Tightening;

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 ? int.Parse(args[1]) : 4545;

var services = new ServiceCollection();

services.AddLogging(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

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
            o.Terminator = Ddon.OpenProtocol.Configuration.MessageTerminator.None;
        });

        endpoint.MapResponse<Mid0060, Mid0061>();
        endpoint.MapResponse<Mid0062, Mid0005>();
        endpoint.MapResponse<Mid0001, Mid0002>();
        endpoint.MapResponse<Mid0003, Mid0005>();
        endpoint.MapResponse<Mid0018, Mid0005>();
    });

    return manager;
});

var sp = services.BuildServiceProvider();
var manager = sp.GetRequiredService<IOpenProtocolManager>();
var logger = sp.GetRequiredService<ILogger<Program>>();
var endpoint = manager.GetEndpoint("扭紧机")!;

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

endpoint.SubscribeAll(mid =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ← MID{mid.Header.Mid:D4}");
    return Task.CompletedTask;
});

Console.WriteLine($"正在连接 {host}:{port}...");
await endpoint.StartAsync();
Console.WriteLine("连接成功.");

Console.WriteLine("正在订阅拧紧结果 (MID0060)...");
var firstResult = await endpoint.SubscribeAsync<Mid0061>(new Mid0060());
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine(
    $"[首个结果] 扭矩={firstResult.Torque:F2}Nm " +
    $"角度={firstResult.Angle:F0}度 " +
    $"状态={(firstResult.TighteningStatus ? "OK" : "NOK")}");
Console.ResetColor();

Console.WriteLine();
Console.WriteLine("=== Open Protocol 交互控制台 ===");
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
                Console.WriteLine($"状态: {((OpenProtocolEndpoint)endpoint).State}");
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

await endpoint.StopAsync();
Console.WriteLine("已断开连接.");
