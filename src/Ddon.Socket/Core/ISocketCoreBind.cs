using System;
using System.Threading.Tasks;
using Ddon.Socket.Exceptions;

namespace Ddon.Socket.Core
{
    public interface IDdonSocketSessionBind
    {
        IDdonSocketSessionBind BindByteHandler(Func<SocketCoreSession, Memory<byte>, Task>? byteHandler);

        IDdonSocketSessionBind BindStringHandler(Func<SocketCoreSession, string, Task>? stringHandler);

        IDdonSocketSessionBind BindExceptionHandler(Func<SocketCoreSession, SocketException, Task>? exceptionHandler);

        IDdonSocketSessionBind BindDisconnectHandler(Func<SocketCoreSession, Task>? disconnectHandler);
    }

    public interface IDdonSocketServerBind
    {
        IDdonSocketServerBind BindByteHandler(Func<SocketCoreSession, Memory<byte>, Task>? byteHandler);

        IDdonSocketServerBind BindStringHandler(Func<SocketCoreSession, string, Task>? stringHandler);

        IDdonSocketServerBind BindExceptionHandler(Func<SocketCoreSession, SocketException, Task>? exceptionHandler);

        IDdonSocketServerBind BindDisconnectHandler(Func<SocketCoreSession, Task>? disconnectHandler);

        IDdonSocketServerBind BindConnectHandler(Func<SocketCoreSession, Task>? connectHandler);
    }
}
