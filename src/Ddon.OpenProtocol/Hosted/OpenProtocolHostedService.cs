using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Ddon.OpenProtocol.Hosted
{
    public class OpenProtocolHostedService : BackgroundService
    {
        private readonly IOpenProtocolManager _manager;

        public OpenProtocolHostedService(IOpenProtocolManager manager)
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
