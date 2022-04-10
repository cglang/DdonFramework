using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core;
using Ddon.Socket.Route;
using System.Net;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class DdonSocketFactoryServer<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private readonly TcpListener _listener;

        public static DdonSocketStorage SocketStorage => DdonSocketStorage.GetInstance();

        private DdonSocketFactoryServer(string host, int post)
        {
            _listener = new TcpListener(IPAddress.Parse(host), post);
        }

        public static DdonSocketFactoryServer<TDdonSocketRouteMapLoadBase> CreateServer(IServiceProvider serviceProvider, string host, int post)
        {
            ServiceProviderFactory.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            return new DdonSocketFactoryServer<TDdonSocketRouteMapLoadBase>(host, post);
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

                    Task.Run(async () =>
                    {
                        try
                        {
                            await connection.ConsecutiveReadStreamAsync();
                        }
                        catch (DdonSocketDisconnectException ex)
                        {
                            SocketStorage.Remove(ex.SocketId);
                        }
                    });
                }
            });
        }
    }
}