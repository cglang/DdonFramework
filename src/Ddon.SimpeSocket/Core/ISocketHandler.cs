using System;
using System.Threading.Tasks;
using Ddon.SimpeSocket.Exceptions;
using Ddon.Socket.Core;

namespace Ddon.SimpeSocket.Core
{
    public interface ISocketSessionHandler
    {
        Task ExceptionHandler(SocketSession session, SocketException exception);

        Task StringHandler(SocketSession session, string text);

        Task ByteHandler(SocketSession session, ReadOnlyMemory<byte> data);

        Task DisconnectHandler(SocketSession session);
    }

    public interface ISocketServerCoreHandler : ISocketSessionHandler
    {
        Task ConnectHandler(SocketSession session);
    }
}
