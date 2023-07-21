using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Handler;
using Ddon.Socket.Options;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.Hosting;

namespace Ddon.Socket.Hosting
{
    public abstract class SocketBackgroundService : BackgroundService
    {
        private readonly SocketServerHandler _handle;

        public SocketBackgroundService(SocketServerHandler handle)
        {
            _handle = handle;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var socketBuilderContext = Configure();

            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();

            var sersver = new SocketServer(socketBuilderContext, _handle);

            return sersver.StartAsync(stoppingToken);
        }

        protected abstract SocketServerOption Configure();
    }
}
