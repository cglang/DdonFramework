using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ddon.Core.Use.Socket;
using Ddon.Core.Use.Socket.Exceptions;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket.Handler;

public class SocketServerHandler : IDdonSocketServerHandler
{
    private readonly ILogger<SocketServerHandler> _logger;
    private readonly DdonSocketInvoke _socketInvoke;

    public SocketServerHandler(ILogger<SocketServerHandler> logger, DdonSocketInvoke socketInvoke)
    {
        _logger = logger;
        _socketInvoke = socketInvoke;
    }

    public Task StringHandler(DdonSocketSession session, string text)
    {
        throw new NotImplementedException();
    }

    public async Task ByteHandler(DdonSocketSession session, Memory<byte> bytes)
    {
        try
        {
            var headLength = BitConverter.ToInt32(bytes.Span[..sizeof(int)]);
            var headBytes = bytes.Slice(sizeof(int), headLength);
            var dataBytes = bytes[(sizeof(int) + headLength)..];

            var head = SerializeHelper.JsonDeserialize<DdonSocketSessionHeadInfo>(headBytes) ?? throw new Exception("消息中不包含消息头");
            if (head.Mode == DdonSocketMode.Response)
            {
                ResponseHandle(new DdonSocketPackageInfo<Memory<byte>>(session, head, dataBytes));
                return;
            }

            (string className, string methodName)? route = DdonSocketRouteMap.Get(head.Route);
            if (route is null) return;

            switch (head.Mode)
            {
                case DdonSocketMode.String:
                    {
                        var data = Encoding.UTF8.GetString(dataBytes.Span);
                        await _socketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, data, session, head);
                        break;
                    }
                case DdonSocketMode.Byte:
                    {
                        var data = Encoding.UTF8.GetString(dataBytes.Span);
                        await _socketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, data, session, head);
                        break;
                    }
                case DdonSocketMode.File:
                    {
                        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "File", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                        Directory.CreateDirectory(filePath);

                        var fullName = Path.Combine(filePath, $"{session.SocketId}.{DateTime.UtcNow:mmssffff}.{head.FileName ?? string.Empty}");
                        await using var fileStream = new FileStream(fullName, FileMode.CreateNew);
                        fileStream.Write(dataBytes.Span);
                        fileStream.Close();

                        await _socketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, fullName, session, head);


                        break;
                    }
                case DdonSocketMode.Request:
                    {
                        var jsonData = Encoding.UTF8.GetString(dataBytes.Span);
                        var methodReturn = await _socketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, jsonData, session, head);

                        var responseData = new DdonSocketResponse<object>(DdonSocketResponseCode.OK, methodReturn);
                        var methodReturnJsonBytes = SerializeHelper.JsonSerialize(responseData).GetBytes();
                        var responseHeadBytes = head.Response().GetBytes();

                        ByteArrayHelper.MergeArrays(out var sendBytes, BitConverter.GetBytes(responseHeadBytes.Length), responseHeadBytes, methodReturnJsonBytes);
                        await session.SendBytesAsync(sendBytes);
                        break;
                    }
                case DdonSocketMode.Response:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ByteHandler 错误");
        }
    }

    public Task ConnectHandler(DdonSocketSession session)
    {
        // TODO:优化这个存储类 考虑支持多线程读写的 和 改为静态类
        SessionStorage.Instance.Add(new SocketSession(session));
        return Task.CompletedTask;
    }

    public Task DisconnectHandler(DdonSocketSession session)
    {
        throw new NotImplementedException();
    }

    public Task ExceptionHandler(DdonSocketSession session, DdonSocketException exception)
    {
        throw new NotImplementedException();
    }

    private static void ResponseHandle(DdonSocketPackageInfo<Memory<byte>> info)
    {
        var id = info.Head.Id;

        if (!DdonSocketResponsePool.ContainsKey(id)) return;

        var responseHandle = DdonSocketResponsePool.Get(id);
        if (responseHandle.IsCompleted)
        {
            return;
        }

        var res = SerializeHelper.JsonDeserialize<DdonSocketResponse<object>>(info.Data);

        if (res != null)
        {
            switch (res.Code)
            {
                case DdonSocketResponseCode.OK:
                    responseHandle.ActionThen(SerializeHelper.JsonSerialize(res.Data));
                    break;
                case DdonSocketResponseCode.Error:
                    responseHandle.ExceptionThen(SerializeHelper.JsonSerialize(res.Data));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        DdonSocketResponsePool.Remove(id);
    }
}
