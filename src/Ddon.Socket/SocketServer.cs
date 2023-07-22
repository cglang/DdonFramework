using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Core;
using Ddon.Socket.Core.Storage;
using Ddon.Socket.Options;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Handler;

namespace Ddon.Socket
{
    public class SocketServer
    {
        private readonly SocketCoreServer _server;

        public SocketServer(SocketServerOption option, SocketServerHandler handle, ISocketCoreSessionStorage sessionStorage)
        {
            _server = DdonSocket.CreateServer(option.IPEndPoint, handle, sessionStorage);
        }


        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            DdonSocketResponsePool.Start();
            return _server.StartAsync(cancellationToken);
        }
    }
}
