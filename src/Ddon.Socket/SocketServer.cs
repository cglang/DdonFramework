using Ddon.Core.Services.LazyService.Static;
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
    public class SocketServer<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private readonly TcpListener _listener;
        private readonly IServiceProvider _serviceProvider;

        private ILogger? Logger => _serviceProvider.GetService<ILogger<SocketServer<TDdonSocketRouteMapLoadBase>>>();

        public static DdonSocketStorage SocketStorage => DdonSocketStorage.GetInstance();

        internal SocketServer(IServiceProvider serviceProvider, string host, int post)
        {
            _listener = new TcpListener(IPAddress.Parse(host), post);
            _serviceProvider = serviceProvider;
        }

        public static SocketServer<TDdonSocketRouteMapLoadBase> CreateServer(IServiceProvider serviceProvider, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            return new SocketServer<TDdonSocketRouteMapLoadBase>(serviceProvider, "127.0.0.1", post);
        }

        public void Start()
        {
            Task.Run(() =>
            {
                _listener.Start();

                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var session = new SocketSession(client);
                    SocketStorage.Add(session);
                    Logger?.LogInformation("客户端接入：{SocketId}", session.Conn.SocketId);
                }
            });
        }
    }


    public class SocketServer : SocketServer<DeafultDdonSocketRouteMap>
    {
        protected SocketServer(IServiceProvider serviceProvider, string host, int post) : base(serviceProvider, host, post)
        {
        }

        public static SocketServer CreateServer(IServiceProvider serviceProvider, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();
            return new SocketServer(serviceProvider, "127.0.0.1", post);
        }
    }
}