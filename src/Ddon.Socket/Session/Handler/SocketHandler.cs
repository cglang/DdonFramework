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

    public Task ByteHandler(SocketSession session, Memory<byte> bytes)
    {
        try
        {
            var (headinfo, data) = GetBody(bytes);

            return headinfo.Mode switch
            {
                SocketMode.String => ModeOfStringAsync(session, headinfo, data),
                SocketMode.Byte => ModeOfByteAsync(session, headinfo, data),
                SocketMode.File => ModeOfFileAsync(headinfo, data),
                SocketMode.Request => ModeOfRequestAsync(headinfo, data),
                SocketMode.Response => ModeOfResponse(session, headinfo, data),
                _ => Task.CompletedTask,
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ByteHandler 错误");
        }

        return Task.CompletedTask;

        (SocketSessionHeadInfo headinfo, Memory<byte> data) GetBody(Memory<byte> bytes)
        {
            // head部分数据的大小       存放一些数据头部的数据       真正的数据
            // [ headSize :: 4bytes ] [ head :: headSize bytes ] [ data ]
            var bodySize = BitConverter.ToInt32(bytes.Slice(0, sizeof(int)).Span);
            var headBytes = bytes.Slice(sizeof(int), bodySize);
            var dataBytes = bytes[(sizeof(int) + bodySize)..];

            var headinfo = SocketSerialize.Deserialize<SocketSessionHeadInfo>(headBytes)
                ?? throw new Exception("消息中不包含消息头");

            return (headinfo, dataBytes);
        }

        async Task ModeOfStringAsync(SocketSession session, SocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            (string className, string methodName)? route = SocketRouteMap.Get(headinfo.Route);
            if (route is null) return;

            var textdata = Encoding.UTF8.GetString(data.Span);
            await Pipeline.ExecuteAsync(new(session, headinfo));
            await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, textdata, session, headinfo);
        }

        async Task ModeOfByteAsync(SocketSession session, SocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            (string className, string methodName)? route = SocketRouteMap.Get(headinfo.Route);
            if (route is null) return;

            var textdata = Encoding.UTF8.GetString(data.Span);
            await Pipeline.ExecuteAsync(new(session, headinfo));
            await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, textdata, session, headinfo);
        }

        Task ModeOfFileAsync(SocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            // TODO: File 需要有一个抽象层接口

            (string className, string methodName)? route = SocketRouteMap.Get(headinfo.Route);
            if (route is null)
                return Task.CompletedTask;
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "File", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(filePath);

            var fullName = Path.Combine(filePath, $"{session.SessionId}.{DateTime.UtcNow:mmssffff}.{headinfo.FileName ?? string.Empty}");
            using var fileStream = new FileStream(fullName, FileMode.CreateNew);
            fileStream.Write(data.Span);
            fileStream.Close();

            return SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, fullName, session, headinfo);
        }

        async Task ModeOfRequestAsync(SocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            (string className, string methodName)? route = SocketRouteMap.Get(headinfo.Route);
            if (route is null) return;
            var jsonData = Encoding.UTF8.GetString(data.Span);
            var methodReturn = await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, jsonData, session, headinfo);

            var responseData = new SocketResponse<object>(SocketResponseCode.OK, methodReturn);
            var methodReturnJsonBytes = SocketSerialize.SerializeOfByte(responseData);

            var responseHeadBytes = SocketSerialize.SerializeOfByte(headinfo.Response());

            await session.SendBytesAsync(BitConverter.GetBytes(responseHeadBytes.Length), responseHeadBytes, methodReturnJsonBytes);
        }

        Task ModeOfResponse(SocketSession session, SocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            if (!TimeoutRecordProcessor.ContainsKey(headinfo.Id))
                return Task.CompletedTask;

            var responseHandle = TimeoutRecordProcessor.Get(headinfo.Id);
            if (responseHandle.IsCompleted)
                return Task.CompletedTask;

            var res = SocketSerialize.Deserialize<SocketResponse<object>>(data);

            if (res != null)
            {
                switch (res.Code)
                {
                    case SocketResponseCode.OK:
                        responseHandle.ActionThen(SocketSerialize.SerializeOfString(res.Data));
                        break;
                    case SocketResponseCode.Error:
                        var ex = new DdonSocketRequestException(SocketSerialize.SerializeOfString(res.Data));
                        responseHandle.ExceptionThen(ex);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            TimeoutRecordProcessor.Remove(headinfo.Id);

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
