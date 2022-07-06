using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Socket
{
    public class DdonSocketServer<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private readonly TcpListener _listener;
        private readonly IServiceProvider _serviceProvider;

        private ILogger? Logger => _serviceProvider.GetService<ILogger<DdonSocketServer<TDdonSocketRouteMapLoadBase>>>();

        public static DdonSocketStorage SocketStorage => DdonSocketStorage.GetInstance();

        private DdonSocketServer(IServiceProvider serviceProvider, string host, int post)
        {
            _listener = new TcpListener(IPAddress.Parse(host), post);
            _serviceProvider = serviceProvider;
        }

        public static DdonSocketServer<TDdonSocketRouteMapLoadBase> CreateServer(IServiceProvider serviceProvider, string host, int post)
        {
            DdonServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            return new DdonSocketServer<TDdonSocketRouteMapLoadBase>(serviceProvider, host, post);
        }

        public void Start()
        {
            Task.Run(() =>
            {
                _listener.Start();

                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var session = new DdonSocketSession(client);
                    SocketStorage.Add(session);
                    Logger?.LogInformation("客户端接入：{SocketId}", session.Conn.SocketId);
                }
            });
        }
    }
}