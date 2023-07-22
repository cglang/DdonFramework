using System.Net;
using Ddon.Socket.Core;
using Ddon.Socket.Core.Storage;

namespace Ddon.Socket;

public static class DdonSocket
{
    public static SocketCoreServer CreateServer(IPAddress? ip = null, int port = 6000)
    {
        return new(new(ip ?? IPAddress.Loopback, port), new SocketCoreSessionStorage());
    }

    public static SocketCoreServer CreateServer<TSocketHandler>(int port = 6000) where TSocketHandler : ISocketCoreServerHandler, new()
    {
        return CreateServer(new(IPAddress.Loopback, port), new TSocketHandler(), new SocketCoreSessionStorage());
    }

    internal static SocketCoreServer CreateServer(
        IPEndPoint localEP,
        ISocketCoreServerHandler handle,
        ISocketCoreSessionStorage sessionStorage)
    {
        var server = new SocketCoreServer(localEP, sessionStorage);

        server.BindConnectHandler(handle.ConnectHandler)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return server;
    }


    public static SocketCoreSession CreateSession(string serverhost, int port) => new(serverhost, port);

    public static SocketCoreSession CreateSession(string serverhost, int port, ISocketCoreSessionHandler handle)
    {
        var client = CreateSession(serverhost, port);
        client.BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindDisconnectHandler(handle.DisconnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);

        return client;
    }

    public static SocketCoreSession CreateSession<TSocketHandler>(string serverhost, int port)
        where TSocketHandler : ISocketCoreSessionHandler, new()
        => CreateSession(serverhost, port, new TSocketHandler());

}
