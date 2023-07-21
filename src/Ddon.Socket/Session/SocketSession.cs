using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core.Use.Socket;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session.Model;

namespace Ddon.Socket.Session
{
    public class SocketSession
    {
        public Guid SessionId { get; internal set; }
        private readonly DdonSocketSession _conn;

        public SocketSession(DdonSocketSession socketSession)
        {
            _conn = socketSession;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public async Task SendAsync(string route, object data)
        {
            var requetBytes = new DdonSocketSessionHeadInfo(default, DdonSocketMode.String, route).GetBytes();
            var dataBytes = SerializeHelper.JsonSerialize(data).GetBytes();
            ByteArrayHelper.MergeArrays(out var contentBytes, BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
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
            var dataBytes = SerializeHelper.JsonSerialize(data).GetBytes();
            ByteArrayHelper.MergeArrays(out var contentBytes, BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
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
                return SerializeHelper.JsonDeserialize<T>(resData);
            }
            catch (Exception ex)
            {
                throw new DdonSocketRequestException(resData, "响应结果反序列化失败", ex);
            }
        }

    }
}
