using System.Threading;
using System.Threading.Tasks;
using Ddon.Serial.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Ddon.Serial.Hosted
{
    public class SerialHostedService : BackgroundService
    {
        private readonly ISerialManager _manager;

        public SerialHostedService(ISerialManager manager)
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
