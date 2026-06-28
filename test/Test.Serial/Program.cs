using System.IO.Ports;
using System.Text;
using Ddon.Pipeline;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Core;
using Ddon.Serial.Models;
using Ddon.Serial.Protocols;

if (args.Length < 2)
{
    Console.WriteLine("用法: Test.Serial <发送端COM> <接收端COM>");
    Console.WriteLine("示例: Test.Serial COM3 COM4");
    Console.WriteLine();
    Console.WriteLine("请先使用 com0com 创建一对虚拟串口:");
    Console.WriteLine("  打开 com0com 控制台 → 添加一对 (CNCA0/CNCB0)");
    Console.WriteLine("  或直接使用默认的 COM3/COM4 对");
    return;
}

var sendPortName = args[0];
var listenPortName = args[1];

Console.WriteLine($"发送端: {sendPortName} → 接收端: {listenPortName}");
Console.WriteLine("=".PadRight(50, '='));

// ---- 接收端: 使用 Ddon.Serial 监听 listenPortName ----
var handler = new ConsoleHandler();
var manager = new SerialManager();
manager.AddEndpoint("Listener", endpoint =>
{
    endpoint.Configure(o =>
    {
        o.PortName = listenPortName;
        o.BaudRate = 9600;
    });

    endpoint.UseProtocol<LineProtocol>();

    endpoint.UsePipeline(pipeline =>
    {
        pipeline.Use<LogMiddleware>();
    });

    endpoint.AddHandlerInstance(handler);
});

await manager.StartAllAsync();
Console.WriteLine($"接收端已启动, 监听 {listenPortName}");

// ---- 发送端: 用户输入数据发送到 sendPortName ----
var lineProtocol = new LineProtocol();
using var sender = new SerialPort(sendPortName, 9600, Parity.None, 8, StopBits.One);
sender.Open();
Console.WriteLine($"发送端已打开 {sendPortName}");
Console.WriteLine();
Console.WriteLine("输入要发送的文字内容, 回车发送, 或输入 'exit' 退出:");

while (true)
{
    var input = Console.ReadLine();
    if (input == "exit") break;

    var bytes = lineProtocol.Encode(input!);
    sender.Write(bytes, 0, bytes.Length);
    Console.WriteLine($"  → 已发送: {input}");
}

Console.WriteLine("正在关闭...");
await manager.StopAllAsync();
Console.WriteLine("已退出");

// ---- Middleware ----
public class LogMiddleware : ISerialMiddleware
{
    public int Index { get; set; }

    public async Task InvokeAsync(SerialContext context, PipelineDelegate<SerialContext> next)
    {
        var text = context.GetString();
        Console.WriteLine($"[Middleware] 收到: \"{text}\" ({context.Length} bytes)");
        await next(context);
    }
}

// ---- Handler ----
public class ConsoleHandler : ISerialHandler
{
    public Task HandleAsync(SerialContext context, CancellationToken cancellationToken = default)
    {
        var text = context.GetString();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Handler] 内容: \"{text}\"");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}
