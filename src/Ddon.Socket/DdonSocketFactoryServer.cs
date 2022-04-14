using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core;
using Ddon.Socket.Route;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Socket
{
    public class DdonSocketFactoryServer<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private readonly TcpListener _listener;
        private readonly IServiceProvider _serviceProvider;

        private ILogger? Logger => _serviceProvider.GetService<ILogger<DdonSocketFactoryServer<TDdonSocketRouteMapLoadBase>>>();

        public static DdonSocketStorage SocketStorage => DdonSocketStorage.GetInstance();

        private DdonSocketFactoryServer(IServiceProvider serviceProvider, string host, int post)
        {
            _listener = new TcpListener(IPAddress.Parse(host), post);
            _serviceProvider = serviceProvider;
        }

        public static DdonSocketFactoryServer<TDdonSocketRouteMapLoadBase> CreateServer(IServiceProvider serviceProvider, string host, int post)
        {
            ServiceProviderFactory.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            return new DdonSocketFactoryServer<TDdonSocketRouteMapLoadBase>(serviceProvider, host, post);
        }

        public void Start()
        {
            Task.Run(() =>
            {
                _listener.Start();

                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var connection = new DdonSocketConnectionCore(client);
                    SocketStorage.Add(connection);
                    Logger?.LogInformation("客户端接入：{SocketId}", connection.SocketId);

                    Task.Run(async () =>
                    {
                        try
                        {
                            await connection.ConsecutiveReadStreamAsync();
                        }
                        catch (DdonSocketDisconnectException ex)
                        {
                            SocketStorage.Remove(ex.SocketId);
                            Logger?.LogInformation("客户端断开：{SocketId}", ex.SocketId);
                        }
                    });
                }
            });
        }
    }
}