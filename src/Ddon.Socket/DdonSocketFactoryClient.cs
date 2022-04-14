using Ddon.Core;
using Ddon.Socket.Route;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Socket
{
    public class DdonSocketFactoryClient<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private readonly DdonSocketConnectionCore _clientConnection;

        private DdonSocketFactoryClient(string host, int post)
        {
            var tcpClient = new TcpClient(host, post);
            _clientConnection = new DdonSocketConnectionCore(tcpClient);
        }

        public static DdonSocketFactoryClient<TDdonSocketRouteMapLoadBase> CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            ServiceProviderFactory.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            return new DdonSocketFactoryClient<TDdonSocketRouteMapLoadBase>(host, post);
        }

        public DdonSocketConnectionCore Start()
        {
            Task.Run(() => _clientConnection.ConsecutiveReadStreamAsync());
            return _clientConnection;
        }
    }
}