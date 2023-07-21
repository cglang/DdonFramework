using System.Net;

namespace Ddon.Core.Use.Socket;

public static class DdonSocket
{
    public static DdonSocketServer CreateServer(string host, int port) => new(host, port);

    public static DdonSocketServer CreateServer(int port) => new(IPAddress.Loopback, port);

    public static DdonSocketServer CreateServer(IPAddress ipAddress, int port, IDdonSocketServerHandler handle)
    {
        var server = new DdonSocketServer(ipAddress, port);

        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }

    public static DdonSocketServer CreateServer(string host, int port, IDdonSocketServerHandler handle)
        => CreateServer(IPAddress.Parse(host), port, handle);

    public static DdonSocketServer CreateServer(int port, IDdonSocketServerHandler handle)
    => CreateServer(IPAddress.Loopback, port, handle);

    public static DdonSocketServer CreateServer<TSocketHandler>(string host, int port)
        where TSocketHandler : IDdonSocketServerHandler, new()
        => CreateServer(host, port, new TSocketHandler());

    public static DdonSocketServer CreateServer<TSocketHandler>(int port)
        where TSocketHandler : IDdonSocketServerHandler, new()
        => CreateServer(IPAddress.Loopback, port, new TSocketHandler());


    public static DdonSocketSession CreateClient(string serverhost, int port) => new(serverhost, port);

    public static DdonSocketSession CreateClient<TSocketHandler>(string serverhost, int port)
        where TSocketHandler : IDdonSocketSessionHandler, new()
    {
        var handle = new TSocketHandler();

        var client = CreateClient(serverhost, port);
        client.BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return client;
    }
}
