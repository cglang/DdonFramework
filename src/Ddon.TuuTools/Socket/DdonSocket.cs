using System.Net;
using Ddon.TuuTools.Socket.Handler;

namespace Ddon.TuuTools.Socket;

public static class DdonSocket
{
    public static DdonSocketServer CreateServer(string host, int port) => new(host, port);

    public static DdonSocketServer CreateServer(int port) => new(IPAddress.Loopback, port);

    public static DdonSocketSession CreateClient(string serverhost, int port) => new(serverhost, port);

    public static DdonSocketServer CreateServer<THandlerProvider>(string host, int port)
        where THandlerProvider : IDdonSocketServerHandler, new()
    {
        var handle = new THandlerProvider();

        var server = CreateServer(host, port);
        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.Disconnect)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }

    public static DdonSocketServer CreateServer<THandlerProvider>(int port)
        where THandlerProvider : IDdonSocketServerHandler, new()
    {
        var handle = new THandlerProvider();

        var server = CreateServer(port);
        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.Disconnect)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }

    public static DdonSocketSession CreateClient<THandlerProvider>(string serverhost, int port)
        where THandlerProvider : IDdonSocketSessionHandler, new()
    {
        var handle = new THandlerProvider();

        var client = CreateClient(serverhost, port);
        client.BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.Disconnect)
            .BindExceptionHandler(handle.ExceptionHandler);

        return client;
    }
}
