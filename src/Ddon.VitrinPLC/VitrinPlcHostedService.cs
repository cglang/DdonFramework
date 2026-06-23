using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Ddon.VitrinPLC
{
    internal sealed class VitrinPlcHostedService : IHostedService
    {
        private readonly PlcHub _hub;

        public VitrinPlcHostedService(PlcHub hub) => _hub = hub;

        public Task StartAsync(CancellationToken cancellationToken) =>
            _hub.StartAllAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) =>
            _hub.StopAllAsync(cancellationToken);
    }
}
