using Ddon.Core.Services.LazyService.Static;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Route;
using System;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class SocketClient<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        protected readonly SocketSession session;

        protected SocketClient(string host, int post)
        {
            var tcpClient = new TcpClient(host, post);
            session = new SocketSession(tcpClient);
        }

        public static SocketSession CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            var t = new SocketClient<TDdonSocketRouteMapLoadBase>(host, post);
            return t.session;
        }
    }

    public class SocketClient : SocketClient<DeafultDdonSocketRouteMap>
    {
        protected SocketClient(string host, int post) : base(host, post)
        {
        }

        public new static SocketSession CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();
            var t = new SocketClient(host, post);
            return t.session;
        }
    }
}