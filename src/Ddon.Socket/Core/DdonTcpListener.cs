using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Ddon.Socket.Core.Handle;

namespace Ddon.Socket.Core
{
    public class DdonTcpListener<THandler> : TcpListener where THandler : IDdonTcpClientDataHandler, new()
    {
        private readonly THandler _handler;

        public DdonTcpListener(IPEndPoint localEP) : base(localEP)
        {
            _handler = new THandler();
        }

        public DdonTcpListener(IPAddress localaddr, int port) : base(localaddr, port)
        {
            _handler = new THandler();
        }

        public async ValueTask<DdonTcpClient> AcceptDdonTcpClientAsync(CancellationToken? cancellationToken = default)
        {
            cancellationToken ??= CancellationToken.None;

            var tcpClient = await AcceptTcpClientAsync(cancellationToken.Value);
            var id = Guid.NewGuid();

            return new DdonTcpClient(tcpClient, id, _handler);
        }
    }
}
