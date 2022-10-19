using Ddon.Core.Exceptions;
using Ddon.Core.Services.LazyService.Static;
using Ddon.Core.Use.Socket;
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

        internal static Session.DdonSocketStorage SocketStorage => Session.DdonSocketStorage.GetInstance();

        internal SocketServer(IServiceProvider serviceProvider, string host, int post)
        {
            _listener = new TcpListener(IPAddress.Parse(host), post);
            _serviceProvider = serviceProvider;
        }

        public static SocketServer<TDdonSocketRouteMapLoadBase> CreateServer(IServiceProvider serviceProvider, int post)
        {
            return CreateServer(serviceProvider, "0.0.0.0", post);
        }

        public static SocketServer<TDdonSocketRouteMapLoadBase> CreateServer(IServiceProvider serviceProvider, string host, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            return new SocketServer<TDdonSocketRouteMapLoadBase>(serviceProvider, host, post);
        }

        public void Start()
        {
            Task.Run(() =>
            {
                _listener.Start();

                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var session = new SocketSession(client, ExceptionHandler);
                    SocketStorage.Add(session);
                    Logger?.LogInformation("客户端接入：{SocketId}", session.Conn.SocketId);
                }
            });
        }

        private Func<DdonSocketCore, DdonSocketException, Task> ExceptionHandler => async (conn, ex) =>
        {
            Logger?.LogWarning(ex, "Scoket 客户端断开:{0}", ex.SocketId);

            SocketStorage.Remove(ex.SocketId);

            await Task.CompletedTask;
        };
    }


    public class SocketServer : SocketServer<DeafultDdonSocketRouteMap>
    {
        protected SocketServer(IServiceProvider serviceProvider, string host, int post) : base(serviceProvider, host, post)
        {
        }

        public new static SocketServer CreateServer(IServiceProvider serviceProvider, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();
            return new SocketServer(serviceProvider, "0.0.0.0", post);
        }
    }
}