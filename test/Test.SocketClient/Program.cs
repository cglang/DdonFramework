using System.Text;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Core;
using Ddon.Socket.Extensions;
using Ddon.Socket.Models;
using Microsoft.Extensions.DependencyInjection;

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 ? int.Parse(args[1]) : 8888;

var services = new ServiceCollection();

services.AddSocket(builder =>
{
    builder.AddEndpoint("client", endpoint =>
    {
        endpoint.Configure(o =>
        {
            o.Host = host;
            o.Port = port;
            o.ConnectTimeout = 5000;
        });

        endpoint.UseReconnect<DefaultReconnectStrategy>();
        endpoint.AddHandler<PrintHandler>();
    });
});

services.AddSocketHostedService();

await using var sp = services.BuildServiceProvider();

var manager = sp.GetRequiredService<ISocketManager>();
await manager.StartAllAsync();

var endpoint = manager.GetEndpoint("client")!;

Console.WriteLine($"已连接 {host}:{port}");
Console.WriteLine("输入消息发送，输入 exit 退出");
Console.WriteLine();

while (true)
{
    var input = Console.ReadLine();
    if (input is null or "exit") break;

    var data = Encoding.UTF8.GetBytes(input);
    await endpoint.Worker.SendAsync(data, 0, data.Length);
}

await manager.StopAllAsync();

sealed class PrintHandler : ISocketHandler
{
    public Task HandleAsync(SocketContext context, CancellationToken cancellationToken = default)
    {
        var text = context.GetString();
        Console.WriteLine($"[收到] {text}");
        return Task.CompletedTask;
    }
}
