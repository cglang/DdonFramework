using System;
using System.Text;
using System.Threading.Tasks;
using Ddon.ConvenientSocket.Exceptions;
using Ddon.Socket.Core;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Model;
using Ddon.Socket.Utility;

namespace Ddon.Socket
{
    public static class SocketCoreSessionExtensions
    {
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static async Task SendAsync(this SocketCoreSession session, string route, object data)
        {
            var requetBytes = new DdonSocketSessionHeadInfo(default, DdonSocketMode.String, route).GetBytes();
            var dataBytes = SerializeHelper.JsonSerialize(data).GetBytes();
            ByteArrayHelper.MergeArrays(out var contentBytes, BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
            await session.SendBytesAsync(contentBytes);
        }

        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">请求超时异样</exception>
        private static async Task<string> RequestAsync(this SocketCoreSession session, string route, object data)
        {
            var taskCompletion = new TaskCompletionSource<string>();

            var response = new DdonSocketResponse(taskCompletion.SetResult, _ => taskCompletion.SetException(new DdonSocketRequestException("请求超时")));
            DdonSocketResponsePool.Add(response);

            var requetBytes = new DdonSocketSessionHeadInfo(response.Id, DdonSocketMode.Request, route).GetBytes();
            var dataBytes = SerializeHelper.JsonSerialize(data).GetBytes();
            ByteArrayHelper.MergeArrays(out var contentBytes, BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
            await session.SendBytesAsync(contentBytes);

            return await taskCompletion.Task;
        }

        /// <summary>
        /// 异步请求等待结果
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        /// <exception cref="DdonSocketRequestException">进行Socket请求是发生异常</exception>
        public static async Task<T?> RequestAsync<T>(this SocketCoreSession session, string route, object data)
        {
            var resData = await session.RequestAsync(route, data);
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
