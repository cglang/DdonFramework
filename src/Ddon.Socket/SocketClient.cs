using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Ddon.ConvenientSocket.Exceptions;
using Ddon.Socket.Core;
using Ddon.Socket.Options;
using Ddon.Socket.Serialize;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Handler;
using Ddon.Socket.Session.Model;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket
{
    public class SocketClient : SocketSession
    {
        private readonly SocketClientOption _option;
        private readonly ILogger<SocketClient> _logger;
        private readonly ISocketSerialize _socketSerialize;

        public SocketClient(
            SocketClientOption option,
            SocketSessionHandler handle,
            ILogger<SocketClient> logger,
            ISocketSerialize socketSerialize)
            : base(option.Host, option.Port)
        {
            BindStringHandler(handle.StringHandler);
            BindByteHandler(handle.ByteHandler);
            BindDisconnectHandler(handle.DisconnectHandler);
            BindExceptionHandler(handle.ExceptionHandler);

            if (option.IsReconnection)
                BindDisconnectHandler(ReconnectionHandler);

            _option = option;
            _logger = logger;
            _socketSerialize = socketSerialize;

            TimeoutRecordProcessor.Start();
        }

        private Func<SocketSession, Task> ReconnectionHandler => async session =>
        {
            for (int number = 1; ; number++)
            {
                try
                {
                    Reconnect(new(_option.Host, _option.Port));
                    _logger.LogInformation("断线重连成功,重试次数:{number}", number);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "正在尝试断线重连,已重试次数:{number}", number);
                    await Task.Delay(100);
                }
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
            var requetBytes = _socketSerialize.SerializeOfByte(new DdonSocketSessionHeadInfo(default, DdonSocketMode.String, route));
            var dataBytes = _socketSerialize.SerializeOfByte(data);
            await SendBytesAsync(BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
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

            var response = new RequestEventListener(taskCompletion.SetResult, 
                _ => taskCompletion.SetException(new DdonSocketRequestException("请求超时")));
            TimeoutRecordProcessor.Add(response);

            var requetBytes = _socketSerialize.SerializeOfByte(new DdonSocketSessionHeadInfo(response.Id, DdonSocketMode.Request, route));
            var dataBytes = _socketSerialize.SerializeOfByte(data);
            await SendBytesAsync(BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);

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
                return _socketSerialize.Deserialize<T>(resData);
            }
            catch (Exception ex)
            {
                throw new DdonSocketRequestException(resData, "响应结果反序列化失败", ex);
            }
        }
    }
}
