using System.Net;
using Ddon.Socket.Core;
using Ddon.Socket.Core.Storage;

namespace Ddon.Socket;

public static class DdonSocket
{
    public static SocketServerCore CreateServer(IPAddress? ip = null, int port = 6000)
    {
        return new(new(ip ?? IPAddress.Loopback, port), new SocketSessionStorage());
    }

    public static SocketServerCore CreateServer<TSocketHandler>(int port = 6000) where TSocketHandler : ISocketServerCoreHandler, new()
    {
        return CreateServer(new(IPAddress.Loopback, port), new TSocketHandler(), new SocketSessionStorage());
    }

    internal static SocketServerCore CreateServer(
        IPEndPoint localEP,
        ISocketServerCoreHandler handle,
        ISocketSessionStorage sessionStorage)
    {
        var server = new SocketServerCore(localEP, sessionStorage);

        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }


    public static SocketSession CreateSession(string serverhost, int port) => new(serverhost, port);

    public static SocketSession CreateSession(string serverhost, int port, ISocketSessionHandler handle)
    {
        var client = CreateSession(serverhost, port);
        client.BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return client;
    }

    public static SocketSession CreateSession<TSocketHandler>(string serverhost, int port)
        where TSocketHandler : ISocketSessionHandler, new()
        => CreateSession(serverhost, port, new TSocketHandler());

}
