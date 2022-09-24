using System.Text;
using Ddon.Core.Use.Socket;

DdonSocket.CreateServer("0.0.0.0", 12333)
    .ConnectHandler(async conn =>
    {
        Console.WriteLine($"客户端接入：{conn.SocketId}");
        await Task.CompletedTask;
    })
    .ByteHandler(async (c, b) =>
    {
        Console.WriteLine($"byte数据:{Encoding.UTF8.GetString(b)}");
        await Task.CompletedTask;
    })
    .StringHandler(async (c, t) =>
    {
        Console.WriteLine($"string数据:{t}");
        await Task.CompletedTask;
    })
    .ExceptionHandler((c, e) =>
    {
        Console.WriteLine($"自定义异常:{e.Message}");
        return Task.CompletedTask;
    })
    .Start();


using (var conn = DdonSocket.CreateClient("127.0.0.1", 12333))
{
    await conn.SendStringAsync("A");
    await conn.SendStringAsync("AB");
    await conn.SendStringAsync("ABC");
    await conn.SendStringAsync("ABCD");
    
    
    await conn.SendBytesAsync(Encoding.UTF8.GetBytes("A"));
    await conn.SendBytesAsync(Encoding.UTF8.GetBytes("AB"));
    await conn.SendBytesAsync(Encoding.UTF8.GetBytes("ABC"));
    await conn.SendBytesAsync(Encoding.UTF8.GetBytes("ABCD"));

    await Task.Delay(100);
}

// Console.ReadKey();