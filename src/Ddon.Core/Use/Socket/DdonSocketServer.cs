using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Ddon.TuuTools.Socket.Exceptions;
using Ddon.TuuTools.Socket.Handler;

namespace Ddon.TuuTools.Socket;

public class DdonSocketServer : DdonSocketServerBase
{
    private readonly TcpListener _listener;

    public DdonSocketServer(string host, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(host), port);
    }

    public DdonSocketServer(IPAddress ipAddress, int port)
    {
        _listener = new TcpListener(ipAddress, port);
    }

    protected override async Task Function()
    {
        _listener.Start();
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();

            var session = new DdonSocketSession(client, Guid.NewGuid());

            session.BindByteHandler(ByteHandler)
                .BindStringHandler(StringHandler)
                .BindExceptionHandler(ExceptionHandler)
                .BindExceptionHandler(DefaultExceptionHandler)
                .BindDisconnectHandler(DisconnectHandler);

            await session.SendBytesAsync(session.SocketId.ToByteArray(), 0);

            DdonSocketStorage.Add(session);

            if (ConnectHandler != null)
                await ConnectHandler(session);
            else
                throw new Exception();
        }
    }

    private static readonly Func<DdonSocketSession, DdonSocketException, Task>? DefaultExceptionHandler = (_, ex) =>
    {
        DdonSocketStorage.Remove(ex.SocketId);
        return Task.CompletedTask;
    };
}
