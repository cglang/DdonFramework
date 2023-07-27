using Ddon.Socket.Options;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Handler;
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
        private readonly SocketClientOption _options;

        public SocketClientFactory(
            SocketSessionHandler handler,
            IOptions<SocketClientOption> options,
            ILogger<SocketClient> logger,
            ISocketSerialize socketSerialize)
        {
            _handler = handler;
            _logger = logger;
            _socketSerialize = socketSerialize;
            _options = options.Value;
        }

        public SocketClient Create()
        {
            SocketRouteMap.Init();

            return new(_options, _handler, _logger, _socketSerialize);
        }
    }
}
