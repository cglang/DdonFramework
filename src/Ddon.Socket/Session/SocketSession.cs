using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core.Exceptions;
using Ddon.Core.Services.LazyService.Static;
using Ddon.Core.Use;
using Ddon.Core.Use.Socket;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Route;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    public class SocketSession : IDisposable
    {
        public DdonSocketCore Conn { get; }

        private static IServiceProvider ServiceProvider => LazyServiceProvider.LazyServicePrivider.ServiceProvider;

        private static DdonSocketInvoke SocketInvoke => ServiceProvider.GetRequiredService<DdonSocketInvoke>();

        private static ILogger Logger => ServiceProvider.GetRequiredService<ILogger<SocketSession>>();

        public SocketSession(TcpClient tcpClient, Func<DdonSocketCore, DdonSocketException, Task> exceptionHandler)
        {
            Conn = new DdonSocketCore(tcpClient, ByteHandler, exceptionHandler);
        }

        private static void ResponseHandle(DdonSocketPackageInfo<string> info)
        {
            var pairs = DdonSocketResponsePool.GetInstance().Pairs;
            var id = info.Head.Id;
            if (pairs.ContainsKey(id))
            {
                var res = DdonSocketCore.JsonDeserialize<DdonSocketResponse<object>>(info.Data);

                if (res != null)
                {
                    if (res.Code == DdonSocketResponseCode.OK)
                        pairs[id].ActionThen?.Invoke(DdonSocketCore.JsonSerialize(res.Data));
                    else if (res.Code == DdonSocketResponseCode.Error)
                        pairs[id].ExceptionThen?.Invoke(DdonSocketCore.JsonSerialize(res.Data));
                }
                else
                    pairs[id].ExceptionThen?.Invoke("响应数据错误");

                pairs.Remove(id);
            }
        }

        private Func<DdonSocketCore, byte[], Task> ByteHandler => async (conn, bytes) =>
        {
            var headBytes = DdonArray.ByteCut(bytes[0..DdonSocketConst.HeadLength]);
            var dataBytes = bytes[DdonSocketConst.HeadLength..];

            var head = DdonSocketCore.JsonDeserialize<DdonSocketRequest>(Encoding.UTF8.GetString(headBytes)) ?? throw new Exception("消息中不包含消息头");
            if (head.Mode == DdonSocketMode.Response)
            {
                var textData = Encoding.UTF8.GetString(dataBytes);
                ResponseHandle(new DdonSocketPackageInfo<string>(conn, head, textData));
                return;
            }

            var route = DdonSocketRouteMap.Get(head.Api);
            if (route is null) return;

            if (head.Mode == DdonSocketMode.String)
            {
                var data = Encoding.UTF8.GetString(dataBytes);
                await SocketInvoke.IvnvokeAsync(route.Value.Item1, route.Value.Item2, data, this, head);
            }
            else if (head.Mode == DdonSocketMode.Byte)
            {
                var data = Encoding.UTF8.GetString(dataBytes);
                await SocketInvoke.IvnvokeAsync(route.Value.Item1, route.Value.Item2, data, this, head);
            }
            else if (head.Mode == DdonSocketMode.File)
            {
                var tmpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "Tmp", head.Id.ToString());
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Socket", "File");

                if (head.IsEnd)
                {
                    var jsonData = Encoding.UTF8.GetString(dataBytes);
                    var fileinfo = DdonSocketCore.JsonDeserialize<DdonSocketFileInfo>(jsonData);

                    Directory.CreateDirectory(filePath);

                    var fileWrite = new FileStream(Path.Combine(filePath, $"{head.Id}.{fileinfo?.FileName ?? string.Empty}"), FileMode.OpenOrCreate);

                    foreach (var file in Directory.GetFiles(tmpPath, "*.tmp"))
                    {
                        // TODO 这里还有很大的bug  传大文件的时候 合不起来 现在只能发送小于 DdonSocketConst.FileLength 的文件
                        using var fs = new FileStream(file, FileMode.Open);
                        fileWrite.Position = fileWrite.Length;
                        fileWrite.Write(fs.ReadAllBytes());
                        fs.Close();
                    }

                    Directory.Delete(tmpPath, true);

                    await SocketInvoke.IvnvokeAsync(route.Value.Item1, route.Value.Item2, "", this, head);
                }
                else
                {
                    Directory.CreateDirectory(tmpPath);

                    var fullName = Path.Combine(tmpPath, $"{head.Id}.{head.BlockIndex}.tmp");
                    using var fileStream = new FileStream(fullName, FileMode.CreateNew);
                    fileStream.Position = fileStream.Length;
                    fileStream.Write(dataBytes);
                    fileStream.Close();
                }
            }
            else if (head.Mode == DdonSocketMode.Request)
            {
                var jsonData = Encoding.UTF8.GetString(dataBytes);
                var methodReturn = await SocketInvoke.IvnvokeAsync(route.Value.Item1, route.Value.Item2, jsonData, this, head);

                var responseData = new DdonSocketResponse<object>(DdonSocketResponseCode.OK, methodReturn);
                var methodReturnJsonBytes = DdonSocketCore.JsonSerialize(responseData).GetBytes();
                var responseHeadBytes = head.Response().GetBytes();

                DdonArray.MergeArrays(out var sendBytes, responseHeadBytes, methodReturnJsonBytes, DdonSocketConst.HeadLength);
                await Conn.SendBytesAsync(sendBytes);
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
            var requetBytes = new DdonSocketRequest(default, DdonSocketMode.String, route).GetBytes();
            var dataBytes = DdonSocketCore.JsonSerialize(data).GetBytes();
            DdonArray.MergeArrays(out byte[] contentBytes, requetBytes, dataBytes, DdonSocketConst.HeadLength);
            await Conn.SendBytesAsync(contentBytes);
        }

        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">请求超时异样</exception>
        public async Task<string> RequestAsync(string route, object data)
        {
            var taskCompletion = new TaskCompletionSource<string>();

            var id = Guid.NewGuid();
            var response = new DdonSocketResponseHandler(id);
            response.Then(info => { taskCompletion.SetResult(info); });
            response.Exception(info => { taskCompletion.SetException(new DdonSocketRequestException()); });

            var requetBytes = new DdonSocketRequest(id, DdonSocketMode.Request, route).GetBytes();
            var dataBytes = DdonSocketCore.JsonSerialize(data).GetBytes();
            DdonArray.MergeArrays(out var contentBytes, requetBytes, dataBytes, DdonSocketConst.HeadLength);
            await Conn.SendBytesAsync(contentBytes);

            return await taskCompletion.Task;
        }

        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">请求超时异样</exception>
        public async Task<T?> RequestAsync<T>(string route, object data)
        {
            var resData = await RequestAsync(route, data);
            return DdonSocketCore.JsonDeserialize<T>(resData);
        }

        public async Task SendFileAsync(string route, string filePath)
        {
            var request = new DdonSocketRequest(Guid.NewGuid(), DdonSocketMode.File, route);
            using var source = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] bytes = new byte[DdonSocketConst.FileLength];
            byte[] requetBytes;


            bool end = false;
            int bytesRead;
            while (!end && (bytesRead = source.Read(bytes, 0, DdonSocketConst.FileLength)) > 0)
            {
                if (bytesRead != DdonSocketConst.FileLength)
                {
                    bytes = bytes[0..bytesRead];
                    end = true;
                }
                requetBytes = request.CountAddOne().GetBytes();

                DdonArray.MergeArrays(out var cb, requetBytes, bytes, DdonSocketConst.HeadLength);
                await Conn.SendBytesAsync(cb);
            }

            requetBytes = request.SetEnd().GetBytes();
            var fileinfo = new DdonSocketFileInfo()
            {
                FileName = Path.GetFileName(source.Name),
                FileSize = source.Length
            };

            var filbytes = DdonSocketCore.JsonSerialize(fileinfo).GetBytes();
            DdonArray.MergeArrays(out var contentBytes, requetBytes, filbytes, DdonSocketConst.HeadLength);
            await Conn.SendBytesAsync(contentBytes);
        }

        public void Dispose()
        {
            Conn.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
