﻿using System.Text;
using Ddon.Socket;

var result = string.Empty;
var server = DdonSocket.CreateServer("127.0.0.1", 5356);

server.BindConnectHandler((session) =>
{
    return Task.Run(() => Console.WriteLine($"服务端：连接接入：{session.SessionId}"));
})
.BindStringHandler(async (session, text) =>
{
    Console.WriteLine($"服务端：接收到文本数据：{text}");
    await session.SendStringAsync(text);
})
.BindByteHandler(async (session, data) =>
{
    Console.WriteLine($"服务端：接收到Byte数据：{data.Length},{Encoding.UTF8.GetString(data.Span)}");
    await session.SendBytesAsync(data.ToArray());
})
.BindExceptionHandler((session, ex) =>
{
    return Task.Run(() => Console.WriteLine($"服务端：出现异常：{ex.Message}"));
})
.BindDisconnectHandler((session) =>
{
    return Task.Run(() => Console.WriteLine($"服务端：连接断开：{session.SessionId}"));
});

server.Start();

using var session = DdonSocket.CreateSession<DdonSocketSessionHandler>("127.0.0.1", 5356);

string text = "测试文本";
await session.SendStringAsync(text);

var textByte = Encoding.UTF8.GetBytes(text);
await session.SendBytesAsync(textByte);

await Task.Delay(1000);

session.Dispose();

await Task.Delay(1000);

Console.WriteLine("=======结束=======");
