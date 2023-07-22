using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Core.Storage;

namespace Ddon.Socket.Core;

public class SocketCoreServer : SocketCoreServerBase
{
    private readonly TcpListener _listener;
    private readonly ISocketCoreSessionStorage _sessionStorage;

    public SocketCoreServer(IPEndPoint localEP, ISocketCoreSessionStorage sessionStorage)
    {
        _listener = new TcpListener(localEP);
        _sessionStorage = sessionStorage;
    }

    protected override async Task StartAccept(CancellationToken cancellationToken = default)
    {
        _listener.Start();
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);

            var session = new SocketCoreSession(client, Guid.NewGuid());

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

    private Task DefaultExceptionHandler(SocketCoreSession _, Exceptions.SocketException ex)
    {
        _sessionStorage.Remove(ex.SocketId);
        return Task.CompletedTask;
    }
}
