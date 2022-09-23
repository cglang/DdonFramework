using Ddon.Core.Use.Socket;

DdonSocket.CreateServer("0.0.0.0", 12333)
    .ConnectHandler(async conn =>
    {
        Console.WriteLine($"客户端接入：{conn.SocketId}");
        await Task.CompletedTask;
    })
    .ByteHandler(async (c, b) =>
    {
        Console.WriteLine("byte数据");
        await Task.CompletedTask;
    })
    .StringHandler(async (c, t) =>
    {
        Console.WriteLine($"string数据:{t}");
        await Task.CompletedTask;
    })
    .ExceptionHandler((c, e) =>
    {
        Console.WriteLine("自定义异常");
        return Task.CompletedTask;
    })
    .Start();

await Task.Delay(10000);

Console.ReadLine();
