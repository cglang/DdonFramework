using Ddon.TuuTools.Socket.Exceptions;

namespace Ddon.TuuTools.Socket.Handler
{
    public interface IDdonSocketCoreHandlerProvider
    {
        Task ExceptionHandler(DdonSocketCore session, DdonSocketException exception);

        Task StringHandler(DdonSocketCore session, string data);

        Task ByteHandler(DdonSocketCore session, Memory<byte> data);
    }

    public interface IDdonSocketServerHandlerProvider : IDdonSocketCoreHandlerProvider
    {
        Task ConnectHandler(DdonSocketCore session);
    }
}
