using Ddon.Core.Exceptions;
using Ddon.Core.Use.Socket;
using Ddon.Socket.Session;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Socket.Hosting
{
    // TODO: IHost 还需要看
    public sealed class SocketApplication
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;
        private readonly Func<DdonSocketCore, DdonSocketException, Task>? _exceptionHandler;
        private readonly Func<DdonSocketCore, IServiceProvider, Task>? _socketAccessHandler;
        private readonly TcpListener _listener;

        private readonly Action<TcpClient, IServiceProvider, ILogger?, 
            Func<DdonSocketCore, DdonSocketException, Task>?, 
            Func<DdonSocketCore, IServiceProvider, Task>?> acceptTcpClientHandler =
            (tcpClient, serviceProvider, logger, exceptionHandler, socketAccessHandler) =>
        {
            var session = new SocketSession(tcpClient, exceptionHandler);
            // TODO:优化这个存储类 考虑支持多线程读写的 和 改为静态类
            Session.DdonSocketStorage.GetInstance().Add(session);
            socketAccessHandler?.Invoke(session.Conn, serviceProvider);
        };

        public SocketApplication(
            IServiceProvider serviceProvider,
            TcpListener listener,
            ILogger? Logger,
            Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler,
            Func<DdonSocketCore, IServiceProvider, Task>? socketAccessHandler)
        {
            _serviceProvider = serviceProvider;
            _listener = listener;
            _logger = Logger;
            _exceptionHandler = exceptionHandler;
            _socketAccessHandler = socketAccessHandler;
        }
        public void Run()
        {
            Task.Run(() =>
            {
                _listener.Start();

                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    acceptTcpClientHandler.Invoke(client, _serviceProvider, _logger, _exceptionHandler, _socketAccessHandler);
                }
            });
        }

        public static SocketApplicationBuilder CreateBuilder(string[] args, IServiceProvider serviceProvider)
        {
            return new(serviceProvider);
        }
    }
}
