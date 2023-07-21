using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket;
using Ddon.Socket.Handler;
using Ddon.Socket.Options;
using Ddon.Socket.Session;

namespace Ddon.Socket
{
    public class SocketServer
    {
        private readonly SocketCoreServer _server;

        public SocketServer(SocketServerOption option, SocketServerHandler handle)
        {
            _server = DdonSocket.CreateServer(option.Host, option.Port, handle);
        }


        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            DdonSocketResponsePool.Start();
            return _server.StartAsync();
        }
    }
}
