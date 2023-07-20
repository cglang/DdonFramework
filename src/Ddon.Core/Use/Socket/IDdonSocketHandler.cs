using System;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket.Exceptions;

namespace Ddon.Core.Use.Socket
{
    public interface IDdonSocketSessionHandler
    {
        Task ExceptionHandler(DdonSocketSession session, DdonSocketException exception);

        Task StringHandler(DdonSocketSession session, string text);

        Task ByteHandler(DdonSocketSession session, Memory<byte> data);

        Task Disconnect(DdonSocketSession session);
    }

    public interface IDdonSocketServerHandler : IDdonSocketSessionHandler
    {
        Task ConnectHandler(DdonSocketSession session);
    }
}
