using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Ddon.Socket
{
    public class DdonSocketServer<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandlerCore, new()
    {
        private readonly IServiceProvider? _services;

        private TcpListener Server { get; set; }

        private readonly ILogger<DdonSocketServer<TDdonSocketHandler>> _logger;

        private DdonSocketServer(string host, int post, IServiceProvider services)
        {
            Server = new TcpListener(IPAddress.Parse(host), post);

            _services = services;
            _logger = services.GetRequiredService<ILogger<DdonSocketServer<TDdonSocketHandler>>>();
        }

        private DdonSocketServer(string host, int post,
            ILogger<DdonSocketServer<TDdonSocketHandler>> logger)
        {
            Server = new TcpListener(IPAddress.Parse(host), post);

            _logger = logger;
        }

        public static DdonSocketServer<TDdonSocketHandler> CreateServer(string host, int post, IServiceProvider services)
        {
            return new DdonSocketServer<TDdonSocketHandler>(host, post, services);
        }

        public static DdonSocketServer<TDdonSocketHandler> CreateServer(string host, int post,
            ILogger<DdonSocketServer<TDdonSocketHandler>> logger)
        {
            return new DdonSocketServer<TDdonSocketHandler>(host, post, logger);
        }

        public void Start()
        {
            _logger.LogInformation("监听客户端接入请求...");
            Server.Start();

            _ = Task.Run(() =>
            {
                while (true)
                {
                    var client = Server.AcceptTcpClient();
                    var clientConnection = new DdonSocketConnection<TDdonSocketHandler>(client, _services!);
                    DdonSocketClientConnections<TDdonSocketHandler>.GetInstance().Add(clientConnection);

                    _logger.LogInformation("接收到客户端:{}", clientConnection.ClientId);

                    // 接受来自客户端的数据
                    _ = Task.Run(async () =>
                    {
                        var clientId = await clientConnection.ConsecutiveReadStreamAsync();
                        DdonSocketClientConnections<TDdonSocketHandler>.GetInstance().Remove(clientId);
                        _logger.LogInformation("客户端断开连接:{}", clientId);
                    });
                }
            });
        }
    }
}