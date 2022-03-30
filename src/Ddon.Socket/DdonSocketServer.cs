using Ddon.Socket.Connection;
using Ddon.Socket.Exceptions;
using Ddon.Socket.Handler;
using System.Net;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class DdonSocketServer<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandler, new()
    {
        private readonly TcpListener _listener;

        public static DdonSocketStorage<TDdonSocketHandler> SocketStorage => DdonSocketStorage<TDdonSocketHandler>.GetInstance();

        private DdonSocketServer(string host, int post)
        {
            _listener = new TcpListener(IPAddress.Parse(host), post);
        }

        public static DdonSocketServer<TDdonSocketHandler> CreateServer(string host, int post)
        {
            return new DdonSocketServer<TDdonSocketHandler>(host, post);
        }

        public void Start()
        {
            _listener.Start();

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                var connection = new DdonSocketConnectionServer<TDdonSocketHandler>(client);
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