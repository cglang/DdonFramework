using Ddon.Core;
using Ddon.Socket.Connection;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class DdonSocketFactoryClient
    {
        private readonly DdonSocketConnectionCore _clientConnection;

        private DdonSocketFactoryClient(string host, int post)
        {
            var tcpClient = new TcpClient(host, post);
            _clientConnection = new DdonSocketConnectionCore(tcpClient);
        }

        public static DdonSocketFactoryClient CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            ServiceProviderFactory.InitServiceProvider(serviceProvider);
            return new DdonSocketFactoryClient(host, post);
        }

        public DdonSocketConnectionCore Start()
        {
            Task.Run(() => _clientConnection.ConsecutiveReadStreamAsync());
            return _clientConnection;
        }
    }
}