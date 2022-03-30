using Ddon.Socket.Connection;
using Ddon.Socket.Handler;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Socket
{
    public class DdonSocketClient<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandler, new()
    {
        [AllowNull]
        private static DdonSocketConnectionClient<TDdonSocketHandler> _clientConnection;

        private DdonSocketClient(string host, int post)
        {
            _clientConnection = new DdonSocketConnectionClient<TDdonSocketHandler>(host, post);
        }

        public static DdonSocketClient<TDdonSocketHandler> CreateClient(string host, int post)
        {
            return new DdonSocketClient<TDdonSocketHandler>(host, post);
        }

        public DdonSocketConnectionClient<TDdonSocketHandler> Start()
        {
            Task.Run(() => _clientConnection.ConsecutiveReadStreamAsync());
            return _clientConnection;
        }
    }
}