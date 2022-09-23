using Ddon.Core.Exceptions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Socket
{
    public class DdonSocketServer
    {
        private readonly TcpListener _listener;

        protected Func<DdonSocketCore, Task>? _connectHandler;
        protected Func<DdonSocketCore, byte[], Task>? _byteHandler;
        protected Func<DdonSocketCore, string, Task>? _stringHandler;
        protected Func<DdonSocketCore, DdonSocketException, Task>? _exceptionHandler;

        public DdonSocketServer(string host, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(host), port);
        }

        public DdonSocketServer ByteHandler(Func<DdonSocketCore, byte[], Task>? byteHandler)
        {
            _byteHandler = byteHandler;
            return this;
        }

        public DdonSocketServer StringHandler(Func<DdonSocketCore, string, Task>? stringHandler)
        {
            _stringHandler += stringHandler;
            return this;
        }

        public DdonSocketServer ExceptionHandler(Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler)
        {
            _exceptionHandler += exceptionHandler;
            return this;
        }

        public DdonSocketServer ConnectHandler(Func<DdonSocketCore, Task>? connectHandler)
        {
            _connectHandler += connectHandler;
            return this;
        }

        public void Start()
        {
            _listener.Start();
            Task.Run(async () =>
            {
                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var session = new DdonSocketCore(client);
                    session.ByteHandler(_byteHandler);
                    session.StringHandler(_stringHandler);
                    session.ExceptionHandler(_exceptionHandler);
                    session.ExceptionHandler(_defaultExceptionHandler);

                    DdonSocketStorage.Add(session);

                    if (_connectHandler != null)
                        await _connectHandler(session);
                }
            });
        }

        private static readonly Func<DdonSocketCore, DdonSocketException, Task>? _defaultExceptionHandler = (conn, ex) =>
        {
            DdonSocketStorage.Remove(ex.SocketId);
            Console.WriteLine($"移除一个：{ex.SocketId}");
            return Task.CompletedTask;
        };
    }
}
