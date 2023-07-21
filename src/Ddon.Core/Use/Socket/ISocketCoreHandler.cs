using System;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket.Exceptions;

namespace Ddon.Core.Use.Socket
{
    public interface ISocketCoreSessionHandler
    {
        Task ExceptionHandler(SocketCoreSession session, SocketException exception);

        Task StringHandler(SocketCoreSession session, string text);

        Task ByteHandler(SocketCoreSession session, Memory<byte> data);

        Task DisconnectHandler(SocketCoreSession session);
    }

    public interface ISocketCoreServerHandler : ISocketCoreSessionHandler
    {
        Task ConnectHandler(SocketCoreSession session);
    }
}
