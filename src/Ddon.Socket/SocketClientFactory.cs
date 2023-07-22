using Ddon.Socket.Handler;
using Ddon.Socket.Options;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ddon.Socket
{
    public class SocketClientFactory
    {
        private readonly SocketSessionHandler _handler;
        private readonly ILogger<SocketClient> _logger;
        private readonly SocketClientOption _options;

        public SocketClientFactory(SocketSessionHandler handler, IOptions<SocketClientOption> options, ILogger<SocketClient> logger)
        {
            _handler = handler;
            _logger = logger;
            _options = options.Value;
        }

        public SocketClient Create()
        {
            DdonSocketRouteMap.Init();

            var client = new SocketClient(_options, _handler, _logger);

            return client;
        }
    }
}
