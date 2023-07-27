using Ddon.Socket.Options;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Handler;
using Ddon.Socket.Session.Middleware;
using Ddon.Socket.Session.Pipeline;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ddon.Socket
{
    public class SocketClientFactory
    {
        private readonly SocketSessionHandler _handler;
        private readonly ILogger<SocketClient> _logger;
        private readonly ISocketSerialize _socketSerialize;
        private readonly ISocketMiddlewarePipelineRegistrar _pipelineRegistrar;
        private readonly SocketClientOption _option;

        public SocketClientFactory(
            SocketSessionHandler handler,
            IOptions<SocketClientOption> options,
            ILogger<SocketClient> logger,
            ISocketSerialize socketSerialize,
            ISocketMiddlewarePipelineRegistrar pipelineRegistrar)
        {
            _handler = handler;
            _logger = logger;
            _socketSerialize = socketSerialize;
            _pipelineRegistrar = pipelineRegistrar;
            _option = options.Value;
        }

        public SocketClient Create()
        {
            SocketRouteMap.Init();

            // 管道配置初始化
            _pipelineRegistrar.AddMiddleware<RouteMiddleware>();
            _pipelineRegistrar.AddMiddleware<EndPointMiddleware>();
            _option.PipelineRegistrar?.Invoke(_pipelineRegistrar);

            return new(_option, _handler, _logger, _socketSerialize);
        }
    }
}
