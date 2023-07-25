using System;
using System.Threading.Tasks;
using Ddon.Socket.Exceptions;

namespace Ddon.Socket.Core
{
    public interface ISocketSessionHandler
    {
        Task ExceptionHandler(SocketSession session, SocketException exception);

        Task StringHandler(SocketSession session, string text);

        Task ByteHandler(SocketSession session, Memory<byte> data);

        Task DisconnectHandler(SocketSession session);
    }

    public interface ISocketServerCoreHandler : ISocketSessionHandler
    {
        Task ConnectHandler(SocketSession session);
    }
}
