using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket;
using Ddon.Socket.Handler;
using Ddon.Socket.Hosting;
using Ddon.Socket.Session;

namespace Ddon.Socket
{
    public class SocketServer
    {
        private readonly DdonSocketServer _server;

        public SocketServer(IServiceProvider serviceProvider, SocketServerOption option, SocketServerHandler handle)
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
