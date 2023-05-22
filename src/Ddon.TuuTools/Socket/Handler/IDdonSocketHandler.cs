using Ddon.TuuTools.Socket.Exceptions;

namespace Ddon.TuuTools.Socket.Handler
{
    public interface IDdonSocketSessionHandler
    {
        Task ExceptionHandler(DdonSocketSession session, DdonSocketException exception);

        Task StringHandler(DdonSocketSession session, string data);

        Task ByteHandler(DdonSocketSession session, Memory<byte> data);

        Task Disconnect(DdonSocketSession session);
    }

    public interface IDdonSocketServerHandler : IDdonSocketSessionHandler
    {
        Task ConnectHandler(DdonSocketSession session);
    }
}
