using Ddon.TuuTools.Socket.Exceptions;

namespace Ddon.TuuTools.Socket.Handler
{
    public interface IDdonSocketSessionBindHandler
    {
        IDdonSocketSessionBindHandler BindByteHandler(Func<DdonSocketSession, Memory<byte>, Task>? byteHandler);

        IDdonSocketSessionBindHandler BindStringHandler(Func<DdonSocketSession, string, Task>? stringHandler);

        IDdonSocketSessionBindHandler BindExceptionHandler(Func<DdonSocketSession, DdonSocketException, Task>? exceptionHandler);

        IDdonSocketSessionBindHandler BindDisconnectHandler(Func<DdonSocketSession, Task>? disconnectHandler);
    }

    public interface IDdonSocketServerBindHandler : IDdonSocketSessionBindHandler
    {
        IDdonSocketServerBindHandler BindConnectHandler(Func<DdonSocketSession, Task>? connectHandler);
    }
}
