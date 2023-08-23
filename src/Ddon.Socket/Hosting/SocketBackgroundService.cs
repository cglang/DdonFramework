using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Core.Storage;
using Ddon.Socket.Options;
using Ddon.Socket.Session.Handler;
using Ddon.Socket.Session.Middleware;
using Ddon.Socket.Session.Pipeline;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ddon.Socket.Hosting
{
    public class SocketBackgroundService : BackgroundService
    {
        private readonly SocketServerHandler _handle;
        private readonly SocketServerOption _option;
        private readonly ISocketSessionStorage _sessionStorage;
        private readonly ISocketMiddlewarePipelineRegistrar _pipelineRegistrar;

        public SocketBackgroundService(
            SocketServerHandler handle,
            IOptions<SocketServerOption> option,
            ISocketSessionStorage sessionStorage,
            ISocketMiddlewarePipelineRegistrar pipelineRegistrar)
        {
            _handle = handle;
            _option = option.Value;
            _sessionStorage = sessionStorage;
            _pipelineRegistrar = pipelineRegistrar;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SocketRouteMap.Init();

            _option.PipelineRegistrar?.Invoke(_pipelineRegistrar);

            var sersver = new SocketServer(_option, _handle, _sessionStorage);

            return sersver.StartAsync(stoppingToken);
        }
    }
}
