using System.Net;
using Ddon.Socket.Core;

namespace Ddon.Socket;

public static class DdonSocket
{
    public static SocketCoreServer CreateServer(string host, int port) => new(host, port);

    public static SocketCoreServer CreateServer(int port) => new(IPAddress.Loopback, port);

    public static SocketCoreServer CreateServer(IPAddress ipAddress, int port, ISocketCoreServerHandler handle)
    {
        var server = new SocketCoreServer(ipAddress, port);

        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }

    public static SocketCoreServer CreateServer(string host, int port, ISocketCoreServerHandler handle)
        => CreateServer(IPAddress.Parse(host), port, handle);

    public static SocketCoreServer CreateServer(int port, ISocketCoreServerHandler handle)
        => CreateServer(IPAddress.Loopback, port, handle);

    public static SocketCoreServer CreateServer<TSocketHandler>(string host, int port)
        where TSocketHandler : ISocketCoreServerHandler, new()
        => CreateServer(host, port, new TSocketHandler());

    public static SocketCoreServer CreateServer<TSocketHandler>(int port)
        where TSocketHandler : ISocketCoreServerHandler, new()
        => CreateServer(IPAddress.Loopback, port, new TSocketHandler());


    public static SocketCoreSession CreateClient(string serverhost, int port) => new(serverhost, port);

    public static SocketCoreSession CreateClient(string serverhost, int port, ISocketCoreSessionHandler handle)
    {
        var client = CreateClient(serverhost, port);
        client.BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return client;
    }

    public static SocketCoreSession CreateClient<TSocketHandler>(string serverhost, int port)
        where TSocketHandler : ISocketCoreSessionHandler, new()
        => CreateClient(serverhost, port, new TSocketHandler());

}
