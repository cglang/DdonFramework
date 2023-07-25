using System.Text;
using Ddon.Socket.Core;
using Ddon.Socket.Exceptions;

class DdonSocketSessionHandler : ISocketSessionHandler
{
    public Task ByteHandler(SocketSession session, Memory<byte> data)
    {
        return Task.Run(() => Console.WriteLine($"客户端：接收到Byte数据：{data.Length},{Encoding.UTF8.GetString(data.Span)}"));
    }

    public Task DisconnectHandler(SocketSession session)
    {
        return Task.Run(() => Console.WriteLine($"客户端：连接断开：{session.SessionId}"));
    }

    public Task ExceptionHandler(SocketSession session, SocketException exception)
    {
        return Task.Run(() => Console.WriteLine($"客户端：出现异常：{exception.Message}"));
    }

    public Task StringHandler(SocketSession session, string text)
    {
        return Task.Run(() => Console.WriteLine($"客户端：接收到文本数据：{text}"));
    }
}
