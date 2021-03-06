using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core.Use;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Session.Route;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    public class DdonSocketSession
    {
        public readonly DdonSocketCore Conn;

        private readonly IServiceProvider ServiceProvider = Ddon.Core.Services.LazyService.Static.LazyServiceProvider.LazyServicePrivider.ServiceProvider;

        public DdonSocketSession(TcpClient tcpClient)
        {
            Conn = new DdonSocketCore(tcpClient, ByteHandler, ExceptionHandler);
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
                await DdonSocketInvoke.IvnvokeAsync(ServiceProvider, api.Value.Item1, api.Value.Item2, data, this, head);
            }
            else if (head.Mode == DdonSocketMode.Byte)
            {
                var data = Encoding.UTF8.GetString(dataBytes);
                await DdonSocketInvoke.IvnvokeAsync(ServiceProvider, api.Value.Item1, api.Value.Item2, data, this, head);
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
                await DdonSocketInvoke.IvnvokeAsync(ServiceProvider, api.Value.Item1, api.Value.Item2, fileStream, this, head);
            }
            else if (head.Mode == DdonSocketMode.Request)
            {
                if (api is not null)
                {
                    var data = Encoding.UTF8.GetString(dataBytes);
                    var json = JsonSerializer.Serialize(data);
                    var resData = await DdonSocketInvoke.IvnvokeReturnJsonAsync(ServiceProvider, api.Value.Item1, api.Value.Item2, json, this, head);
                    var resdataBytes = Encoding.UTF8.GetBytes(resData);
                    var resheadBytes = head.Response().GetBytes();
                    byte[] contentBytes = DdonArray.MergeArrays(headBytes, dataBytes, DdonSocketConst.HeadLength);
                    await Conn.SendBytesAsync(contentBytes);
                }
            }
        };

        private Func<DdonSocketCore, Exception, Task> ExceptionHandler => async (conn, ex) =>
        {
            // TODO: Socket 断开等异常时
            await Task.CompletedTask;
        };

        public async Task SendAsync<T>(string route, T data, Guid id = default)
        {
            if (id == default) id = Guid.NewGuid();

            var requetBytes = new DdonSocketRequest(id, DdonSocketMode.String, route).GetBytes();
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
        public async Task<string> RequestAsync(string route, string data)
        {
            var taskCompletion = new TaskCompletionSource<string>();

            var id = Guid.NewGuid();
            await SendAsync(route, data, id);

            var response = new DdonSocketResponseBody(id);
            response.Then(info => { taskCompletion.SetResult(info); })
                .Exception(info => { taskCompletion.SetException(new DdonSocketRequestException()); });

            return await taskCompletion.Task;
        }

        /// <summary>
        /// 响应处理
        /// </summary>
        /// <param name="id"></param>
        /// <param name="info"></param>
        internal void ResponseHandle(DdonSocketPackageInfo<string> info)
        {
            var pairs = DdonSocketResponsePool.GetInstance().Pairs;
            var id = info.Head.Id;
            if (pairs.ContainsKey(id))
            {
                var res = JsonSerializer.Deserialize<DdonSocketResponse<object>>(info.Data);

                if (res != null)
                {
                    if (res.Code == DdonSocketResponseCode.OK)
                        pairs[id].ActionThen?.Invoke(info.Data);
                    else if (res.Code == DdonSocketResponseCode.Error)
                        pairs[id].ExceptionThen?.Invoke(info.Data);
                }

                pairs[id].ExceptionThen?.Invoke("响应数据错误");
            }
        }

        /// <summary>
        /// 从流中获取消息头
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">获取消息头错误</exception>
        private async Task<DdonSocketRequest> ReadHeadAsync()
        {
            var initialBytes = await Conn.Stream.ReadLengthBytesAsync(DdonSocketConst.HeadLength);
            var bytes = DdonArray.ByteCut(initialBytes);

            var headDto = JsonSerializer.Deserialize<DdonSocketRequest>(Encoding.UTF8.GetString(bytes))
                ?? throw new Exception("消息中不包含消息头");

            return headDto;
        }

    }
}
