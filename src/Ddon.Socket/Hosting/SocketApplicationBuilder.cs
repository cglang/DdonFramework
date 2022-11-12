using Ddon.Core.Services.LazyService.Static;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace Ddon.Socket.Hosting
{
    public class SocketApplicationBuilder
    {
        private readonly SocketBuilderContext _socketBuilderContext = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;
        private TcpListener? _tcpListener;


        public SocketApplicationBuilder(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<SocketApplication>>();
            _serviceProvider = serviceProvider;
        }

        public SocketApplication Build()
        {
            LazyServiceProvider.InitServiceProvider(_serviceProvider);
            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();

            _tcpListener = new TcpListener(IPAddress.Parse(_socketBuilderContext.Host), _socketBuilderContext.Port);

            return new(_serviceProvider, _tcpListener, _logger, _socketBuilderContext.ExceptionHandler, _socketBuilderContext.SocketAccessHandler);
        }

        public SocketApplicationBuilder Configure(Action<SocketBuilderContext> configureDelegate)
        {
            configureDelegate.Invoke(_socketBuilderContext);
            return this;
        }
    }
}
