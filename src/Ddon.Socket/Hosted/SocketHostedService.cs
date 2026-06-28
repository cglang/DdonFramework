using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Ddon.Socket.Hosted
{
    public class SocketHostedService : BackgroundService
    {
        private readonly ISocketManager _manager;

        public SocketHostedService(ISocketManager manager)
        {
            _manager = manager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _manager.StartAllAsync(stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _manager.StopAllAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
