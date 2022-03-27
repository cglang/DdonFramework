using Ddon.Socket.Connection;
using Ddon.Socket.Handler;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Socket
{
    public class DdonSocketClientFactory<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandlerBase, new()
    {
        [AllowNull]
        private static DdonSocketConnectionClient<TDdonSocketHandler> _clientConnection;

        private DdonSocketClientFactory(string host, int post)
        {
            _clientConnection = new DdonSocketConnectionClient<TDdonSocketHandler>(host, post);
        }

        public static DdonSocketClientFactory<TDdonSocketHandler> CreateClient(string host, int post)
        {
            return new DdonSocketClientFactory<TDdonSocketHandler>(host, post);
        }

        public DdonSocketConnectionClient<TDdonSocketHandler> Start()
        {
            _ = _clientConnection.ConsecutiveReadStreamAsync();
            return _clientConnection;
        }
    }
}