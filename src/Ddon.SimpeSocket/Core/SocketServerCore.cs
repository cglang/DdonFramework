using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ddon.SimpeSocket.Core.Storage;
using Ddon.SimpeSocket.Exceptions;
using Ddon.Socket.Core;

namespace Ddon.SimpeSocket.Core
{
    public class SocketServerCore : SocketServerCoreBase
    {
        private readonly TcpListener _listener;
        private readonly ISocketSessionStorage _sessionStorage;

        public SocketServerCore(IPEndPoint localEP, ISocketSessionStorage sessionStorage)
        {
            _listener = new TcpListener(localEP);
            _sessionStorage = sessionStorage;
        }

        protected override async Task StartAccept(CancellationToken cancellationToken = default)
        {
            _listener.Start();
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();

                var session = new SocketSession(client, Guid.NewGuid());

                session.BindByteHandler(ByteHandler)
                    .BindStringHandler(StringHandler)
                    .BindExceptionHandler(ExceptionHandler)
                    .BindExceptionHandler(DefaultExceptionHandler)
                    .BindDisconnectHandler(DisconnectHandler);

                _sessionStorage.Add(session);

                if (ConnectHandler != null)
                    await ConnectHandler(session);
            }
        }

        private Task DefaultExceptionHandler(SocketSession _, Exceptions.SocketException ex)
        {
            _sessionStorage.Remove(ex.SocketId);
            return Task.CompletedTask;
        }
    }
}
