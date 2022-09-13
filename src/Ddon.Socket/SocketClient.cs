using Ddon.Core.Services.LazyService.Static;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Route;
using System;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class SocketClient<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private readonly SocketSession _session;

        private SocketClient(string host, int post)
        {
            var tcpClient = new TcpClient(host, post);
            _session = new SocketSession(tcpClient);
        }

        public static SocketSession CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            var t = new SocketClient<TDdonSocketRouteMapLoadBase>(host, post);
            return t._session;
        }
    }
}