using System;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket.Exceptions;

namespace Ddon.Core.Use.Socket
{
    public interface IDdonSocketSessionBindHandler
    {
        IDdonSocketSessionBindHandler BindByteHandler(Func<DdonSocketSession, Memory<byte>, Task>? byteHandler);

        IDdonSocketSessionBindHandler BindStringHandler(Func<DdonSocketSession, string, Task>? stringHandler);

        IDdonSocketSessionBindHandler BindExceptionHandler(Func<DdonSocketSession, DdonSocketException, Task>? exceptionHandler);

        IDdonSocketSessionBindHandler BindDisconnectHandler(Func<DdonSocketSession, Task>? disconnectHandler);
    }

    public interface IDdonSocketServerBindHandler
    {
        IDdonSocketServerBindHandler BindByteHandler(Func<DdonSocketSession, Memory<byte>, Task>? byteHandler);

        IDdonSocketServerBindHandler BindStringHandler(Func<DdonSocketSession, string, Task>? stringHandler);

        IDdonSocketServerBindHandler BindExceptionHandler(Func<DdonSocketSession, DdonSocketException, Task>? exceptionHandler);

        IDdonSocketServerBindHandler BindDisconnectHandler(Func<DdonSocketSession, Task>? disconnectHandler);

        IDdonSocketServerBindHandler BindConnectHandler(Func<DdonSocketSession, Task>? connectHandler);
    }
}
