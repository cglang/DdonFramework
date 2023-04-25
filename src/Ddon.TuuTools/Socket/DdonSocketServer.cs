using System.Net;
using System.Net.Sockets;
using Ddon.TuuTools.Socket.Exceptions;
using Ddon.TuuTools.Socket.Handler;

namespace Ddon.TuuTools.Socket;

public class DdonSocketServer : DdonSocketServerHandler
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

    public DdonSocketServer BindByteHandler(Func<DdonSocketCore, Memory<byte>, Task>? byteHandler)
    {
        ByteHandler += byteHandler;
        return this;
    }

    public DdonSocketServer BindStringHandler(Func<DdonSocketCore, string, Task>? stringHandler)
    {
        StringHandler += stringHandler;
        return this;
    }

    public DdonSocketServer BindExceptionHandler(Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler)
    {
        ExceptionHandler += exceptionHandler;
        return this;
    }

    public DdonSocketServer BindConnectHandler(Func<DdonSocketCore, Task>? connectHandler)
    {
        ConnectHandler += connectHandler;
        return this;
    }

    public void Start()
    {
        Task<Task>.Factory.StartNew(Function, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public Task StartAsync()
    {
        return Function();
    }

    private async Task Function()
    {
        _listener.Start();
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            var session = new DdonSocketCore(client);
            session.BindByteHandler(ByteHandler);
            session.BindStringHandler(StringHandler);
            session.BindExceptionHandler(ExceptionHandler);
            session.BindExceptionHandler(DefaultExceptionHandler);

            DdonSocketStorage.Add(session);

            if (ConnectHandler != null)
                await ConnectHandler(session);
            else
                throw new Exception();
        }
    }

    private static readonly Func<DdonSocketCore, DdonSocketException, Task>? DefaultExceptionHandler = (_, ex) =>
    {
        DdonSocketStorage.Remove(ex.SocketId);
        return Task.CompletedTask;
    };
}
