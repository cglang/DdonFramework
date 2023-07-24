using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ddon.Socket.Core;
using Ddon.Socket.Exceptions;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Route;
using Ddon.Socket.Utility;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket.Session.Handler;

public class SocketSessionHandler : ISocketCoreSessionHandler
{
    protected ILogger<SocketServerHandler> Logger { get; }
    protected SocketInvoke SocketInvoke { get; }
    protected ISocketSerialize SocketSerialize { get; }

    public SocketSessionHandler(ILogger<SocketServerHandler> logger, SocketInvoke socketInvoke, ISocketSerialize socketSerialize)
    {
        Logger = logger;
        SocketInvoke = socketInvoke;
        SocketSerialize = socketSerialize;
    }

    public Task StringHandler(SocketCoreSession session, string text)
    {
        Logger.LogError("现在还无法处理文本类型的数据");
        return Task.CompletedTask;
    }

    public Task ByteHandler(SocketCoreSession session, Memory<byte> bytes)
    {
        try
        {
            var (headinfo, data) = GetBody(bytes);

            return headinfo.Mode switch
            {
                DdonSocketMode.String => ModeOfStringAsync(headinfo, data),
                DdonSocketMode.Byte => ModeOfByteAsync(headinfo, data),
                DdonSocketMode.File => ModeOfFileAsync(headinfo, data),
                DdonSocketMode.Request => ModeOfRequestAsync(headinfo, data),
                DdonSocketMode.Response => ModeOfResponse(session, headinfo, data),
                _ => Task.CompletedTask,
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ByteHandler 错误");
        }

        return Task.CompletedTask;

        (DdonSocketSessionHeadInfo headinfo, Memory<byte> data) GetBody(Memory<byte> bytes)
        {
            // head部分数据的大小       存放一些数据头部的数据       真正的数据
            // [ headSize :: 4bytes ] [ head :: headSize bytes ] [ data ]
            var bodySize = BitConverter.ToInt32(bytes.Slice(0, sizeof(int)).Span);
            var headBytes = bytes.Slice(sizeof(int), bodySize);
            var dataBytes = bytes.Slice(sizeof(int) + bodySize, bytes.Length);

            var headinfo = SocketSerialize.Deserialize<DdonSocketSessionHeadInfo>(headBytes)
                ?? throw new Exception("消息中不包含消息头");

            return (headinfo, dataBytes);
        }

        Task ModeOfStringAsync(DdonSocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            (string className, string methodName)? route = DdonSocketRouteMap.Get(headinfo.Route);
            if (route is null) return Task.CompletedTask;

            var textdata = Encoding.UTF8.GetString(data.Span);
            return SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, textdata, session, headinfo);
        }

        Task ModeOfByteAsync(DdonSocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            (string className, string methodName)? route = DdonSocketRouteMap.Get(headinfo.Route);
            if (route is null) return Task.CompletedTask;

            var textdata = Encoding.UTF8.GetString(data.Span);
            return SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, textdata, session, headinfo);
        }

        Task ModeOfFileAsync(DdonSocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            // TODO: File 需要有一个抽象层接口

            (string className, string methodName)? route = DdonSocketRouteMap.Get(headinfo.Route);
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

        async Task ModeOfRequestAsync(DdonSocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            (string className, string methodName)? route = DdonSocketRouteMap.Get(headinfo.Route);
            if (route is null) return;
            var jsonData = Encoding.UTF8.GetString(data.Span);
            var methodReturn = await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, jsonData, session, headinfo);

            var responseData = new DdonSocketResponse<object>(DdonSocketResponseCode.OK, methodReturn);
            var methodReturnJsonBytes = SocketSerialize.SerializeOfByte(responseData);

            var responseHeadBytes = headinfo.Response().GetBytes();

            ByteArrayHelper.MergeArrays(out var sendBytes, BitConverter.GetBytes(responseHeadBytes.Length), responseHeadBytes, methodReturnJsonBytes.Span);
            await session.SendBytesAsync(sendBytes);
        }

        Task ModeOfResponse(SocketCoreSession session, DdonSocketSessionHeadInfo headinfo, Memory<byte> data)
        {
            if (!DdonSocketResponsePool.ContainsKey(headinfo.Id))
                return Task.CompletedTask;

            var responseHandle = DdonSocketResponsePool.Get(headinfo.Id);
            if (responseHandle.IsCompleted)
                return Task.CompletedTask;

            var res = SocketSerialize.Deserialize<DdonSocketResponse<object>>(data);

            if (res != null)
            {
                switch (res.Code)
                {
                    case DdonSocketResponseCode.OK:
                        responseHandle.ActionThen(SocketSerialize.SerializeOfString(res.Data));
                        break;
                    case DdonSocketResponseCode.Error:
                        responseHandle.ExceptionThen(SocketSerialize.SerializeOfString(res.Data));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            DdonSocketResponsePool.Remove(headinfo.Id);

            return Task.CompletedTask;
        }
    }

    public Task DisconnectHandler(SocketCoreSession session)
    {
        Logger.LogInformation($"连接断开:{session.SessionId}");
        return Task.CompletedTask;
    }

    public Task ExceptionHandler(SocketCoreSession session, SocketException exception)
    {
        Logger.LogError(exception, $"异常");
        return Task.CompletedTask;
    }
}

public class SocketServerHandler : SocketSessionHandler, ISocketCoreServerHandler
{
    public SocketServerHandler(ILogger<SocketServerHandler> logger, SocketInvoke socketInvoke, ISocketSerialize socketSerialize) : base(logger, socketInvoke, socketSerialize)
    {
    }

    public Task ConnectHandler(SocketCoreSession session)
    {
        // TODO:优化这个存储类 考虑支持多线程读写的 和 改为静态类
        Logger.LogInformation($"连接接入:{session.SessionId}");
        return Task.CompletedTask;
    }
}
