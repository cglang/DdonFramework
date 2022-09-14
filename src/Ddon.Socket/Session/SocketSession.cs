using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core.Exceptions;
using Ddon.Core.Services.LazyService.Static;
using Ddon.Core.Use;
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
    public class SocketSession
    {
        public readonly DdonSocketCore Conn;

        private IServiceProvider ServiceProvider => LazyServiceProvider.LazyServicePrivider.ServiceProvider;

        private DdonSocketInvoke SocketInvoke => ServiceProvider.GetRequiredService<DdonSocketInvoke>();

        private ILogger Logger => ServiceProvider.GetRequiredService<ILogger<SocketSession>>();

        public SocketSession(TcpClient tcpClient, Func<DdonSocketCore, DdonSocketException, Task> exceptionHandler)
        {
            Conn = new DdonSocketCore(tcpClient, ByteHandler, exceptionHandler);
        }

        private Func<DdonSocketCore, byte[], Task> ByteHandler => async (conn, bytes) =>
        {
            var headBytes = DdonArray.ByteCut(bytes[0..DdonSocketConst.HeadLength]);
            var dataBytes = DdonArray.ByteCut(bytes[DdonSocketConst.HeadLength..]);

            var head = JsonSerializer.Deserialize<DdonSocketRequest>(Encoding.UTF8.GetString(headBytes)) ?? throw new Exception("消息中不包含消息头");
            if (head.Mode == DdonSocketMode.Response)
            {
                var textData = Encoding.UTF8.GetString(dataBytes);
                ResponseHandle(new DdonSocketPackageInfo<string>(conn, head, textData));
                return;
            }

            var api = DdonSocketRouteMap.Get(head.Api);
            if (api is null) return;

            if (head.Mode == DdonSocketMode.String)
            {
                var data = Encoding.UTF8.GetString(dataBytes);
                await SocketInvoke.IvnvokeAsync(api.Value, data, this, head);
            }
            else if (head.Mode == DdonSocketMode.Byte)
            {
                var data = Encoding.UTF8.GetString(dataBytes);
                await SocketInvoke.IvnvokeAsync(api.Value, data, this, head);
            }
            else if (head.Mode == DdonSocketMode.File)
            {
                // 前 160 byte 作为文件的格式名称，20个字符。
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", DateTime.Now.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(path);
                var fileFormat = Encoding.UTF8.GetString(DdonArray.ByteCut(dataBytes[0..160]));
                var fullName = Path.Combine(path, $"{Guid.NewGuid()}{fileFormat}");

                using var fileStream = new FileStream(fullName, FileMode.CreateNew);
                fileStream.Write(dataBytes, 160, dataBytes.Length - 160);
                await SocketInvoke.IvnvokeAsync(api.Value.Item1, api.Value.Item2, fileStream, this, head);
            }
            else if (head.Mode == DdonSocketMode.Request)
            {
                var jsonData = Encoding.UTF8.GetString(dataBytes);
                var methodReturn = await SocketInvoke.IvnvokeAsync(api.Value, jsonData, this, head);

                var responseData = new DdonSocketResponse<dynamic>(DdonSocketResponseCode.OK, methodReturn);
                var methodReturnJsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(responseData));
                var responseHeadBytes = head.Response().GetBytes();

                var sendBytes = DdonArray.MergeArrays(responseHeadBytes, methodReturnJsonBytes, DdonSocketConst.HeadLength);
                await Conn.SendBytesAsync(sendBytes);
            }
        };

        public async Task SendAsync(string route, object data)
        {
            var requetBytes = new DdonSocketRequest(default, DdonSocketMode.String, route).GetBytes();
            var json = JsonSerializer.Serialize(data);
            var dataBytes = Encoding.UTF8.GetBytes(json);
            byte[] contentBytes = DdonArray.MergeArrays(requetBytes, dataBytes, DdonSocketConst.HeadLength);
            await Conn.SendBytesAsync(contentBytes);
        }

        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">请求超时异样</exception>
        public async Task<T?> RequestAsync<T>(string route, object data)
        {
            var taskCompletion = new TaskCompletionSource<string>();

            var id = Guid.NewGuid();
            var response = new DdonSocketResponseBody(id);
            response.Then(info => { taskCompletion.SetResult(info); })
                .Exception(info => { taskCompletion.SetException(new DdonSocketRequestException()); });

            var requetBytes = new DdonSocketRequest(id, DdonSocketMode.Request, route).GetBytes();
            var json = JsonSerializer.Serialize(data);
            var dataBytes = Encoding.UTF8.GetBytes(json);
            byte[] contentBytes = DdonArray.MergeArrays(requetBytes, dataBytes, DdonSocketConst.HeadLength);
            await Conn.SendBytesAsync(contentBytes);

            var result = await taskCompletion.Task;

            return JsonSerializer.Deserialize<T>(result);
        }

        private static void ResponseHandle(DdonSocketPackageInfo<string> info)
        {
            var pairs = DdonSocketResponsePool.GetInstance().Pairs;
            var id = info.Head.Id;
            if (pairs.ContainsKey(id))
            {
                var res = JsonSerializer.Deserialize<DdonSocketResponse<object>>(info.Data);

                if (res != null)
                {
                    if (res.Code == DdonSocketResponseCode.OK)
                        pairs[id].ActionThen?.Invoke(JsonSerializer.Serialize(res.Data));
                    else if (res.Code == DdonSocketResponseCode.Error)
                        pairs[id].ExceptionThen?.Invoke(JsonSerializer.Serialize(res.Data));
                }

                pairs[id].ExceptionThen?.Invoke("响应数据错误");
            }
        }
    }
}
