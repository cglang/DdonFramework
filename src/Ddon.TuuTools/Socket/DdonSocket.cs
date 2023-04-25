using System.Net;
using Ddon.TuuTools.Socket.Handler;

namespace Ddon.TuuTools.Socket;

public static class DdonSocket
{
    public static DdonSocketServer CreateServer(string host, int port) => new(host, port);

    public static DdonSocketServer CreateServer(int port) => new(IPAddress.Loopback, port);

    public static DdonSocketCore CreateClient(string serverhost, int port) => new(serverhost, port);

    public static DdonSocketServer CreateServer<THandlerProvider>(string host, int port)
        where THandlerProvider : IDdonSocketServerHandlerProvider, new()
    {
        var handle = new THandlerProvider();

        return CreateServer(host, port)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindConnectHandler(handle.ConnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);
    }

    public static DdonSocketServer CreateServer<THandlerProvider>(int port)
        where THandlerProvider : IDdonSocketServerHandlerProvider, new()
    {
        var handle = new THandlerProvider();

        return CreateServer(port)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindConnectHandler(handle.ConnectHandler)
            .BindExceptionHandler(handle.ExceptionHandler);
    }

    public static DdonSocketCore CreateClient<THandlerProvider>(string serverhost, int port)
        where THandlerProvider : IDdonSocketCoreHandlerProvider, new()
    {
        var handle = new THandlerProvider();

        return CreateClient(serverhost, port)
            .BindStringHandler(handle.StringHandler)
            .BindByteHandler(handle.ByteHandler)
            .BindExceptionHandler(handle.ExceptionHandler);
    }
}
