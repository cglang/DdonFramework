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
        private readonly ILogger? _logger;

        public IServiceProvider ServiceProvider { get; }

        public SocketApplicationBuilder(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<SocketApplication>>();
            ServiceProvider = serviceProvider;
        }

        public SocketApplication Build()
        {
            LazyServiceProvider.InitServiceProvider(ServiceProvider);
            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();
            var tcpListener = new TcpListener(IPAddress.Parse(_socketBuilderContext.Host), _socketBuilderContext.Port);
            return new(ServiceProvider, tcpListener, _logger, _socketBuilderContext.ExceptionHandler, _socketBuilderContext.SocketAccessHandler);
        }

        public SocketApplicationBuilder Configure(Action<SocketBuilderContext> configureDelegate)
        {
            configureDelegate.Invoke(_socketBuilderContext);
            return this;
        }
    }
}
