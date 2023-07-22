using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Handler;
using Ddon.Socket.Options;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ddon.Socket.Hosting
{
    public class SocketBackgroundService : BackgroundService
    {
        private readonly SocketServerHandler _handle;
        private readonly SocketServerOption _option;

        public SocketBackgroundService(SocketServerHandler handle, IOptions<SocketServerOption> option)
        {
            _handle = handle;
            _option = option.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DdonSocketRouteMap.Init();

            var sersver = new SocketServer(_option, _handle);

            return sersver.StartAsync(stoppingToken);
        }
    }
}
