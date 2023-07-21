using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Handler;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.Hosting;

namespace Ddon.Socket.Hosting
{
    public abstract class SocketBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SocketServerHandler handle;

        public SocketBackgroundService(IServiceProvider serviceProvider, SocketServerHandler handle)
        {
            _serviceProvider = serviceProvider;
            this.handle = handle;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var socketBuilderContext = Configure();

            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();

            var sersver = new SocketServer(_serviceProvider, socketBuilderContext, handle);

            return sersver.StartAsync(stoppingToken);
        }

        protected abstract SocketServerOption Configure();
    }
}
