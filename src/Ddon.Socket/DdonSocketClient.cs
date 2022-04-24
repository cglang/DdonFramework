using Ddon.Core;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Route;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

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

        public static DdonSocketClient<TDdonSocketRouteMapLoadBase> CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            DdonServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            return new DdonSocketClient<TDdonSocketRouteMapLoadBase>(host, post);
        }

        public DdonSocketSession Start()
        {
            Task.Run(() => _session.Conn.ConsecutiveReadStreamAsync());
            return _session;
        }
    }
}