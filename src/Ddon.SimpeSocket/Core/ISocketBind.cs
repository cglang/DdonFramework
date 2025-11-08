using System;
using System.Threading.Tasks;
using Ddon.SimpeSocket.Exceptions;
using Ddon.Socket.Core;

namespace Ddon.SimpeSocket.Core
{
    public interface ISocketSessionBind
    {
        ISocketSessionBind BindByteHandler(Func<SocketSession, ReadOnlyMemory<byte>, Task> byteHandler);

        ISocketSessionBind BindStringHandler(Func<SocketSession, string, Task> stringHandler);

        ISocketSessionBind BindExceptionHandler(Func<SocketSession, SocketException, Task> exceptionHandler);

        ISocketSessionBind BindDisconnectHandler(Func<SocketSession, Task> disconnectHandler);
    }

    public interface ISocketServerBind
    {
        ISocketServerBind BindByteHandler(Func<SocketSession, ReadOnlyMemory<byte>, Task> byteHandler);

        ISocketServerBind BindStringHandler(Func<SocketSession, string, Task> stringHandler);

        ISocketServerBind BindExceptionHandler(Func<SocketSession, SocketException, Task> exceptionHandler);

        ISocketServerBind BindDisconnectHandler(Func<SocketSession, Task> disconnectHandler);

        ISocketServerBind BindConnectHandler(Func<SocketSession, Task> connectHandler);
    }
}
