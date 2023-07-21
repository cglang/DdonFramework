using System.Text;
using Ddon.Core.Use.Socket;
using Ddon.Core.Use.Socket.Exceptions;

class DdonSocketSessionHandler : ISocketCoreSessionHandler
{
    public Task ByteHandler(SocketCoreSession session, Memory<byte> data)
    {
        return Task.Run(() => Console.WriteLine($"客户端：接收到Byte数据：{data.Length},{Encoding.UTF8.GetString(data.Span)}"));
    }

    public Task DisconnectHandler(SocketCoreSession session)
    {
        return Task.Run(() => Console.WriteLine($"客户端：连接断开：{session.SessionId}"));
    }

    public Task ExceptionHandler(SocketCoreSession session, SocketException exception)
    {
        return Task.Run(() => Console.WriteLine($"客户端：出现异常：{exception.Message}"));
    }

    public Task StringHandler(SocketCoreSession session, string text)
    {
        return Task.Run(() => Console.WriteLine($"客户端：接收到文本数据：{text}"));
    }
}
