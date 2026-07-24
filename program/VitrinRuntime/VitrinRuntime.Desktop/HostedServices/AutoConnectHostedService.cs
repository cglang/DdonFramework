using Microsoft.Extensions.Hosting;
using VitrinRuntime.Desktop.Services;

namespace VitrinRuntime.Desktop.HostedServices
{
    public class AutoConnectHostedService : BackgroundService
    {
        private readonly PlcManager _plcManager;

        public AutoConnectHostedService(PlcManager plcManager)
        {
            _plcManager = plcManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var plcs = _plcManager.ListPlcs().Where(x => x.AutoConnect);
            foreach (var plcsItem in plcs)
            {
                await _plcManager.ConnectPlc(new PlcNameRequest() { Name = plcsItem.Name });
            }
        }
    }
}
