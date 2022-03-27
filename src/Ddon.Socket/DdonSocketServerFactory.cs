using Ddon.Socket.Connection;
using Ddon.Socket.Exceptions;
using Ddon.Socket.Handler;
using System.Net;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class DdonSocketServerFactory<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandlerBase, new()
    {
        private TcpListener Server { get; set; }

        private DdonSocketServerFactory(string host, int post)
        {
            Server = new TcpListener(IPAddress.Parse(host), post);
        }

        public static DdonSocketServerFactory<TDdonSocketHandler> CreateServer(string host, int post)
        {
            return new DdonSocketServerFactory<TDdonSocketHandler>(host, post);
        }

        public void Start()
        {
            Server.Start();

            while (true)
            {
                var client = Server.AcceptTcpClient();
                var connection = new DdonSocketConnectionServer<TDdonSocketHandler>(client);
                DdonSocketStorage<TDdonSocketHandler>.GetInstance().Add(connection);
                Console.WriteLine($"客户端接入{connection.SocketId}");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await connection.ConsecutiveReadStreamAsync();
                    }
                    catch (DdonSocketDisconnectException e)
                    {
                        DdonSocketStorage<TDdonSocketHandler>.GetInstance().Remove(e.SocketId);
                    }
                });
            }
        }
    }
}