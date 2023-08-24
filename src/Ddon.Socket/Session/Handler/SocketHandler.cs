using System;
using System.Threading.Tasks;
using Ddon.Socket.Core;
using Ddon.Socket.Exceptions;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Pipeline;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket.Session.Handler;

public class SocketSessionHandler : ISocketSessionHandler
{
    protected ILogger<SocketServerHandler> Logger { get; }
    protected ISocketSerialize SocketSerialize { get; }
    protected ISocketByteCustomPipeline Pipeline { get; }

    public SocketSessionHandler(
        ILogger<SocketServerHandler> logger,
        ISocketSerialize socketSerialize,
        ISocketByteCustomPipeline pipeline)
    {
        Logger = logger;
        SocketSerialize = socketSerialize;
        Pipeline = pipeline;
    }

    public Task StringHandler(SocketSession session, string text)
    {
        var context = new SocketContext(session, text);
        return Pipeline.ExecuteAsync(context);
    }

    public Task ByteHandler(SocketSession session, ReadOnlyMemory<byte> bytes)
    {
        var context = GetSocketContext(session, bytes);
        return Pipeline.ExecuteAsync(context);
    }

    public Task DisconnectHandler(SocketSession session)
    {
        Logger.LogInformation($"连接断开:{session.SessionId}");
        return Task.CompletedTask;
    }

    public Task ExceptionHandler(SocketSession session, SocketException exception)
    {
        Logger.LogError(exception, $"异常");
        return Task.CompletedTask;
    }

    protected SocketContext GetSocketContext(SocketSession session, ReadOnlyMemory<byte> bytes)
    {
        // head部分数据的大小       存放一些数据头部的数据       真正的数据
        // [ headSize :: 4bytes ] [ head :: headSize bytes ] [ data ]
        var headlength = BitConverter.ToInt32(bytes[..sizeof(int)].Span);
        var head = bytes.Slice(sizeof(int), headlength);
        var data = bytes[(sizeof(int) + headlength)..];

        var headinfo = SocketSerialize.Deserialize<SocketHeadInfo>(head)
            ?? throw new Exception("消息中不包含消息头");

        return new(session, headinfo, data);
    }
}

public class SocketServerHandler : SocketSessionHandler, ISocketServerCoreHandler
{
    public SocketServerHandler(
        ILogger<SocketServerHandler> logger,
        ISocketSerialize socketSerialize,
        ISocketByteCustomPipeline pipeline)
        : base(logger, socketSerialize, pipeline)
    {
    }

    public Task ConnectHandler(SocketSession session)
    {
        Logger.LogInformation($"连接接入:{session.SessionId}");
        return Task.CompletedTask;
    }
}
