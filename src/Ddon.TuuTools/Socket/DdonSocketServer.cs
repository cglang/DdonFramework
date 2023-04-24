using System.Net;
using System.Net.Sockets;
using Ddon.TuuTools.Socket.Exceptions;

namespace Ddon.TuuTools.Socket;

public class DdonSocketServer
{
    private readonly TcpListener _listener;

    private Func<DdonSocketCore, Task>? _connectHandler;
    private Func<DdonSocketCore, Memory<byte>, Task>? _byteHandler;
    private Func<DdonSocketCore, string, Task>? _stringHandler;
    private Func<DdonSocketCore, DdonSocketException, Task>? _exceptionHandler;

    public DdonSocketServer(string host, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(host), port);
    }

    public DdonSocketServer(IPAddress ipAddress, int port)
    {
        _listener = new TcpListener(ipAddress, port);
    }

    public DdonSocketServer ByteHandler(Func<DdonSocketCore, Memory<byte>, Task>? byteHandler)
    {
        _byteHandler = byteHandler;
        return this;
    }

    public DdonSocketServer StringHandler(Func<DdonSocketCore, string, Task>? stringHandler)
    {
        _stringHandler += stringHandler;
        return this;
    }

    public DdonSocketServer ExceptionHandler(Func<DdonSocketCore, DdonSocketException, Task>? exceptionHandler)
    {
        _exceptionHandler += exceptionHandler;
        return this;
    }

    public DdonSocketServer ConnectHandler(Func<DdonSocketCore, Task>? connectHandler)
    {
        _connectHandler += connectHandler;
        return this;
    }

    public void Start()
    {
        _listener.Start();
        Task<Task>.Factory.StartNew(Function, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task Function()
    {
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            var session = new DdonSocketCore(client);
            session.ByteHandler(_byteHandler);
            session.StringHandler(_stringHandler);
            session.ExceptionHandler(_exceptionHandler);
            session.ExceptionHandler(DefaultExceptionHandler);

            DdonSocketStorage.Add(session);

            if (_connectHandler != null)
                await _connectHandler(session);
            else
                throw new Exception();
        }
    }

    private static readonly Func<DdonSocketCore, DdonSocketException, Task>? DefaultExceptionHandler = (_, ex) =>
    {
        DdonSocketStorage.Remove(ex.SocketId);
#if DEBUG
        Console.WriteLine($"移除一个：{ex.SocketId}");
#endif
        return Task.CompletedTask;
    };
}
