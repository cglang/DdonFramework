using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core.Services.LazyService.Static;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Route;
using Ddon.TuuTools.Socket;
using Ddon.TuuTools.Socket.Exceptions;
using Ddon.TuuTools.System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    public class SocketSession : IDisposable
    {
        private readonly Func<SocketSession, DdonSocketException, Task>? _exceptionHandler;
        private readonly DdonSocketCore _conn;

        public Guid SessionId { get; } = Guid.NewGuid();

        private static IServiceProvider ServiceProvider => LazyServiceProvider.LazyServicePrivider.ServiceProvider;

        private static DdonSocketInvoke SocketInvoke => ServiceProvider.GetRequiredService<DdonSocketInvoke>();

        private static ILogger Logger => ServiceProvider.GetRequiredService<ILogger<SocketSession>>();

        private Func<DdonSocketCore, DdonSocketException, Task> ConnExceptionHandler => async (_, ex) =>
        {
            if (_exceptionHandler is not null) await _exceptionHandler.Invoke(this, ex);
        };

        public SocketSession(TcpClient tcpClient, Func<SocketSession, DdonSocketException, Task>? exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
            _conn = new DdonSocketCore(tcpClient, ByteHandler, ConnExceptionHandler);
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

            var res = JsonDeserialize<DdonSocketResponse<object>>(info.Data);

            if (res != null)
            {
                switch (res.Code)
                {
                    case DdonSocketResponseCode.OK:
                        responseHandle.ActionThen(JsonSerialize(res.Data));
                        break;
                    case DdonSocketResponseCode.Error:
                        responseHandle.ExceptionThen(JsonSerialize(res.Data));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            DdonSocketResponsePool.Remove(id);
        }

        private Func<DdonSocketCore, Memory<byte>, Task> ByteHandler => async (conn, bytes) =>
        {
            try
            {
                var headLength = BitConverter.ToInt32(bytes.Span[..sizeof(int)]);
                var headBytes = bytes.Slice(sizeof(int), headLength);
                var dataBytes = bytes[(sizeof(int) + headLength)..];

                var head = JsonDeserialize<DdonSocketSessionHeadInfo>(headBytes) ?? throw new Exception("消息中不包含消息头");
                if (head.Mode == DdonSocketMode.Response)
                {
                    ResponseHandle(new DdonSocketPackageInfo<Memory<byte>>(conn, head, dataBytes));
                    return;
                }

                (string className, string methodName)? route = DdonSocketRouteMap.Get(head.Route);
                if (route is null) return;

                switch (head.Mode)
                {
                    case DdonSocketMode.String:
                    {
                        var data = Encoding.UTF8.GetString(dataBytes.Span);
                        await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, data, this, head);
                        break;
                    }
                    case DdonSocketMode.Byte:
                    {
                        var data = Encoding.UTF8.GetString(dataBytes.Span);
                        await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, data, this, head);
                        break;
                    }
                    case DdonSocketMode.File:
                    {
                        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "File", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                        Directory.CreateDirectory(filePath);

                        var fullName = Path.Combine(filePath, $"{conn.SocketId}.{DateTime.UtcNow:mmssffff}.{head.FileName ?? string.Empty}");
                        await using var fileStream = new FileStream(fullName, FileMode.CreateNew);
                        fileStream.Write(dataBytes.Span);
                        fileStream.Close();

                        await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, fullName, this, head);

                        //var tmpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "Tmp", head.Id.ToString());
                        //var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "File");

                        //if (head.IsEnd)
                        //{
                        //    // TODO: 这里需要确保已经收到了全部的文件 也就是总大小与发送的一样
                        //    var jsonData = Encoding.UTF8.GetString(dataBytes);
                        //    var fileinfo = DdonSocketCore.JsonDeserialize<DdonSocketFileInfo>(jsonData);

                        //    Directory.CreateDirectory(filePath);

                        //    using var fileWrite = new FileStream(Path.Combine(filePath, $"{head.Id}.{fileinfo?.FileName ?? string.Empty}"), FileMode.OpenOrCreate);

                        //    foreach (var file in Directory.GetFiles(tmpPath, "*.tmp"))
                        //    {
                        //        // TODO 这里还有很大的bug  传大文件的时候 合不起来 现在只能发送小于 DdonSocketConst.FileLength 的文件
                        //        using var fs = new FileStream(file, FileMode.Open);
                        //        fileWrite.Position = fileWrite.Length;
                        //        fileWrite.Write(fs.ReadAllBytes());
                        //        fs.Close();
                        //    }

                        //    Directory.Delete(tmpPath, true);
                        //}
                        //else
                        //{
                        //    Directory.CreateDirectory(tmpPath);

                        //    var fullName = Path.Combine(tmpPath, $"{head.Id}.{head.BlockIndex}.tmp");
                        //    using var fileStream = new FileStream(fullName, FileMode.CreateNew);
                        //    fileStream.Position = fileStream.Length;
                        //    fileStream.Write(dataBytes);
                        //    fileStream.Close();
                        //}
                        break;
                    }
                    case DdonSocketMode.Request:
                    {
                        var jsonData = Encoding.UTF8.GetString(dataBytes.Span);
                        var methodReturn = await SocketInvoke.IvnvokeAsync(route.Value.className, route.Value.methodName, jsonData, this, head);

                        var responseData = new DdonSocketResponse<object>(DdonSocketResponseCode.OK, methodReturn);
                        var methodReturnJsonBytes = JsonSerialize(responseData).GetBytes();
                        var responseHeadBytes = head.Response().GetBytes();

                        DdonArray.MergeArrays(out var sendBytes, BitConverter.GetBytes(responseHeadBytes.Length), responseHeadBytes, methodReturnJsonBytes);
                        await _conn.SendBytesAsync(sendBytes);
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
                Logger.LogError(ex, "ByteHandler 错误");
            }
        };

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task SendAsync(string route, object data)
        {
            var requetBytes = new DdonSocketSessionHeadInfo(default, DdonSocketMode.String, route).GetBytes();
            var dataBytes = JsonSerialize(data).GetBytes();
            DdonArray.MergeArrays(out byte[] contentBytes, BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
            await _conn.SendBytesAsync(contentBytes);
        }

        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">请求超时异样</exception>
        private async Task<string> RequestAsync(string route, object data)
        {
            var taskCompletion = new TaskCompletionSource<string>();

            var response = new DdonSocketResponseHandler(taskCompletion.SetResult, _ => taskCompletion.SetException(new DdonSocketRequestException("请求超时")));
            DdonSocketResponsePool.Add(response);

            var requetBytes = new DdonSocketSessionHeadInfo(response.Id, DdonSocketMode.Request, route).GetBytes();
            var dataBytes = JsonSerialize(data).GetBytes();
            DdonArray.MergeArrays(out var contentBytes, BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
            await _conn.SendBytesAsync(contentBytes);

            return await taskCompletion.Task;
        }

        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">进行Socket请求是发生异常</exception>
        public async Task<T?> RequestAsync<T>(string route, object data)
        {
            var resData = await RequestAsync(route, data);
            try
            {
                return JsonDeserialize<T>(resData);
            }
            catch (Exception ex)
            {
                throw new DdonSocketRequestException(resData, "响应结果反序列化失败", ex);
            }
        }

        // TODO: 这里还没有写完善
        public async Task SendFileAsync(string route, string filePath)
        {
            var head = new DdonSocketSessionHeadInfo(Guid.NewGuid(), DdonSocketMode.File, route);
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            head.FileName = Path.GetFileName(fileStream.Name);
            var headBytes = head.GetBytes();

            DdonArray.MergeArrays(out var cb, BitConverter.GetBytes(headBytes.Length), head.GetBytes(), await fileStream.ReadAllBytesAsync());
            await _conn.SendBytesAsync(cb);

            //var request = new DdonSocketRequest(Guid.NewGuid(), DdonSocketMode.File, route);
            //using var source = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            //byte[] bytes = new byte[DdonSocketConst.FileLength];
            //byte[] requetBytes;


            //bool end = false;
            //int bytesRead;
            //while (!end && (bytesRead = source.Read(bytes, 0, DdonSocketConst.FileLength)) > 0)
            //{
            //    if (bytesRead != DdonSocketConst.FileLength)
            //    {
            //        bytes = bytes[0..bytesRead];
            //        end = true;
            //    }
            //    requetBytes = request.CountAddOne().GetBytes();

            //    DdonArray.MergeArrays(out var cb, requetBytes, bytes, DdonSocketConst.HeadLength);
            //    await Conn.SendBytesAsync(cb);
            //}

            //requetBytes = request.SetEnd().GetBytes();
            //var fileinfo = new DdonSocketFileInfo()
            //{
            //    FileName = Path.GetFileName(source.Name),
            //    FileSize = source.Length
            //};

            //var filbytes = DdonSocketCore.JsonSerialize(fileinfo).GetBytes();
            //DdonArray.MergeArrays(out var contentBytes, requetBytes, filbytes, DdonSocketConst.HeadLength);
            //await Conn.SendBytesAsync(contentBytes);
        }

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public static string JsonSerialize<T>(T data)
        {
            return JsonSerializer.Serialize(data, Options);
        }

        private static T? JsonDeserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data, Options);
        }

        public static T? JsonDeserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data, Options);
        }

        private static T? JsonDeserialize<T>(Memory<byte> data)
        {
            return JsonSerializer.Deserialize<T>(data.Span, Options);
        }


        private bool _disposed;

        ~SocketSession() { Dispose(false); }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 清理托管资源
                _conn.Dispose();
                DdonSocketResponsePool.Dispose();
            }

            // 清理非托管资源

            _disposed = true;
        }
    }
}
