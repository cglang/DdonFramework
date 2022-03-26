using System.Net.Sockets;

namespace Ddon.Socket
{
    public class DdonSocketConnection<TDdonSocketHandler> : DdonSocketClient<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandlerCore, new()
    {
        public DdonSocketConnection(TcpClient tcpClient, IServiceProvider service) : base(tcpClient, service) { }

        public new async Task<Guid> ConsecutiveReadStreamAsync() => await base.ConsecutiveReadStreamAsync();
    }
}
