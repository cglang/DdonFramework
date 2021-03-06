using Ddon.Core.Services.LazyService.Static;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Route;
using System;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class DdonSocketClient<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private readonly DdonSocketSession _session;

        private DdonSocketClient(string host, int post)
        {
            var tcpClient = new TcpClient(host, post);
            _session = new DdonSocketSession(tcpClient);
        }

        public static DdonSocketSession CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            var t = new DdonSocketClient<TDdonSocketRouteMapLoadBase>(host, post);
            return t._session;
        }
    }
}