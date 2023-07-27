using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ddon.ConvenientSocket.Exceptions;
using Ddon.Socket.Core;
using Ddon.Socket.Exceptions;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Pipeline;
using Ddon.Socket.Session.Route;
using Ddon.Socket.Utility;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket.Session.Handler;

public class SocketSessionHandler : ISocketSessionHandler
{
    protected ILogger<SocketServerHandler> Logger { get; }
    protected SocketInvoke SocketInvoke { get; }
    protected ISocketSerialize SocketSerialize { get; }
    protected ISocketByteCustomPipeline Pipeline { get; }

    public SocketSessionHandler(
        ILogger<SocketServerHandler> logger,
        SocketInvoke socketInvoke,
        ISocketSerialize socketSerialize,
        ISocketByteCustomPipeline pipeline)
    {
        Logger = logger;
        SocketInvoke = socketInvoke;
        SocketSerialize = socketSerialize;
        Pipeline = pipeline;
    }

    public Task StringHandler(SocketSession session, string text)
    {
        Logger.LogError("现在还无法处理文本类型的数据");
        return Task.CompletedTask;
    }

    public async Task ByteHandler(SocketSession session, Memory<byte> bytes)
    {
        try
        {
            // head部分数据的大小       存放一些数据头部的数据       真正的数据
            // [ headSize :: 4bytes ] [ head :: headSize bytes ] [ data ]
            var headlength = BitConverter.ToInt32(bytes[..sizeof(int)].Span);
            var head = bytes[sizeof(int)..headlength];
            var data = bytes[(sizeof(int) + headlength)..];

            var headinfo = SocketSerialize.Deserialize<SocketSessionHeadInfo>(head)
                ?? throw new Exception("消息中不包含消息头");

            var context = new SocketContext(session, headinfo, data);

            await Pipeline.ExecuteAsync(context);
            await (headinfo.Mode switch
            {
                SocketMode.String => ModeOfStringAsync(context),
                SocketMode.Byte => ModeOfByteAsync(context),
                SocketMode.File => ModeOfFileAsync(context),
                SocketMode.Request => ModeOfRequestAsync(context),
                SocketMode.Response => ModeOfResponse(context),
                _ => Task.CompletedTask,
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ByteHandler 错误");
        }

        async Task ModeOfStringAsync(SocketContext context)
        {
            (string className, string methodName)? route = SocketRouteMap.Get(context.Head.Route);
            if (route is null) return;

            var textdata = Encoding.UTF8.GetString(context.SourceData.Span);
            await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, textdata, session, context.Head);
        }

        async Task ModeOfByteAsync(SocketContext context)
        {
            (string className, string methodName)? route = SocketRouteMap.Get(context.Head.Route);
            if (route is null) return;

            var textdata = Encoding.UTF8.GetString(context.SourceData.Span);
            await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, textdata, session, context.Head);
        }

        Task ModeOfFileAsync(SocketContext context)
        {
            // TODO: File 需要有一个抽象层接口

            (string className, string methodName)? route = SocketRouteMap.Get(context.Head.Route);
            if (route is null)
                return Task.CompletedTask;
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "File", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(filePath);

            var fullName = Path.Combine(filePath, $"{session.SessionId}.{DateTime.UtcNow:mmssffff}.{context.Head.FileName ?? string.Empty}");
            using var fileStream = new FileStream(fullName, FileMode.CreateNew);
            fileStream.Write(context.SourceData.Span);
            fileStream.Close();

            return SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, fullName, session, context.Head);
        }

        async Task ModeOfRequestAsync(SocketContext context)
        {
            (string className, string methodName)? route = SocketRouteMap.Get(context.Head.Route);
            if (route is null) return;
            var jsonData = Encoding.UTF8.GetString(context.SourceData.Span);
            var methodReturn = await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, jsonData, session, context.Head);

            var responseData = new SocketResponse<object>(SocketResponseCode.OK, methodReturn);
            var methodReturnJsonBytes = SocketSerialize.SerializeOfByte(responseData);

            var responseHeadBytes = SocketSerialize.SerializeOfByte(context.Head.Response());

            await session.SendBytesAsync(BitConverter.GetBytes(responseHeadBytes.Length), responseHeadBytes, methodReturnJsonBytes);
        }

        Task ModeOfResponse(SocketContext context)
        {
            if (!TimeoutRecordProcessor.ContainsKey(context.Head.Id))
                return Task.CompletedTask;

            var request = TimeoutRecordProcessor.Get(context.Head.Id);
            if (request.IsCompleted)
                return Task.CompletedTask;

            var res = SocketSerialize.Deserialize<SocketResponse<object>>(context.SourceData);

            if (res != null)
            {
                switch (res.Code)
                {
                    case SocketResponseCode.OK:
                        request.ActionHandler(SocketSerialize.SerializeOfString(res.Data));
                        break;
                    case SocketResponseCode.Error:
                        var ex = new DdonSocketRequestException(SocketSerialize.SerializeOfString(res.Data));
                        request.ExceptionHandler(ex);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return Task.CompletedTask;
        }
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
}

public class SocketServerHandler : SocketSessionHandler, ISocketServerCoreHandler
{
    public SocketServerHandler(
        ILogger<SocketServerHandler> logger,
        SocketInvoke socketInvoke,
        ISocketSerialize socketSerialize,
        ISocketByteCustomPipeline pipeline)
        : base(logger, socketInvoke, socketSerialize, pipeline)
    {
    }

    public Task ConnectHandler(SocketSession session)
    {
        Logger.LogInformation($"连接接入:{session.SessionId}");
        return Task.CompletedTask;
    }
}
