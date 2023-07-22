using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Socket.Core;

public class SocketCoreServer : SocketCoreServerBase
{
    private readonly TcpListener _listener;

    public SocketCoreServer(string host, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(host), port);
    }

    public SocketCoreServer(IPAddress ipAddress, int port)
    {
        _listener = new TcpListener(ipAddress, port);
    }

    protected override async Task Function()
    {
        _listener.Start();
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();

            var session = new SocketCoreSession(client, Guid.NewGuid());

            session.BindByteHandler(ByteHandler)
                .BindStringHandler(StringHandler)
                .BindExceptionHandler(ExceptionHandler)
                .BindExceptionHandler(DefaultExceptionHandler)
                .BindDisconnectHandler(DisconnectHandler);

            SocketStorage.Add(session);

            if (ConnectHandler != null)
                await ConnectHandler(session);

            session.Start();
        }
    }

    private static readonly Func<SocketCoreSession, Exceptions.SocketException, Task>? DefaultExceptionHandler = (_, ex) =>
    {
        SocketStorage.Remove(ex.SocketId);
        return Task.CompletedTask;
    };
}
