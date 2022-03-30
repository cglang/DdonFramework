using Ddon.Socket.Handler;
using System.Net.Sockets;

namespace Ddon.Socket.Connection
{
    // TODO 应该再加个有servicePrivider版的
    public class DdonSocketConnectionServer<TDdonSocketHandler> : DdonSocketConnectionBase<TDdonSocketHandler>
        where TDdonSocketHandler : DdonSocketHandlerBase, new()
    {
        public DdonSocketConnectionServer(TcpClient tcpClient) : base(tcpClient)
        {
            SocketId = Guid.NewGuid();
            Stream.Write(SocketId.ToByteArray(), 0, 16);
        }
    }
}
