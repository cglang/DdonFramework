using System.Net;

namespace Ddon.Core.Use.Socket;

public static class DdonSocket
{
    public static DdonSocketServer CreateServer(string host, int port) => new(host, port);

    public static DdonSocketServer CreateServer(int port) => new(IPAddress.Loopback, port);

    public static DdonSocketServer CreateServer<TSocketHandler>(string host, int port)
        where TSocketHandler : IDdonSocketServerHandler, new()
    {
        var handle = new TSocketHandler();

        var server = CreateServer(host, port);
        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.Disconnect)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }

    public static DdonSocketServer CreateServer<TSocketHandler>(int port)
        where TSocketHandler : IDdonSocketServerHandler, new()
    {
        var handle = new TSocketHandler();

        var server = CreateServer(port);
        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.Disconnect)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }

    public static DdonSocketSession CreateClient(string serverhost, int port) => new(serverhost, port);

    public static DdonSocketSession CreateClient<TSocketHandler>(string serverhost, int port)
        where TSocketHandler : IDdonSocketSessionHandler, new()
    {
        var handle = new TSocketHandler();

        var client = CreateClient(serverhost, port);
        client.BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.Disconnect)
            .BindExceptionHandler(handle.ExceptionHandler);

        return client;
    }
}
