using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core;
using Ddon.Socket.Connection;
using System.Net;
using System.Net.Sockets;

namespace Ddon.ConvenientSocket
{
    public class DdonSocketFactoryServer
    {
        private readonly TcpListener _listener;

        public static DdonSocketStorage SocketStorage => DdonSocketStorage.GetInstance();

        private DdonSocketFactoryServer(string host, int post)
        {
            _listener = new TcpListener(IPAddress.Parse(host), post);
        }

        public static DdonSocketFactoryServer CreateServer(IServiceProvider serviceProvider, string host, int post)
        {
            ServiceProviderFactory.InitServiceProvider(serviceProvider);
            return new DdonSocketFactoryServer(host, post);
        }

        public void Start()
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
        }
    }
}