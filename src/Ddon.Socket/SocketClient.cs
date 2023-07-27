using System;
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
    public class SocketClient : IDisposable
    {
        private readonly SocketSession _session;

        private readonly SocketClientOption _option;
        private readonly ILogger<SocketClient> _logger;
        private readonly ISocketSerialize _socketSerialize;

        public SocketClient(
            SocketClientOption option,
            SocketSessionHandler handle,
            ILogger<SocketClient> logger,
            ISocketSerialize socketSerialize)
        {
            _session = new(option.Host, option.Port);

            _session.BindStringHandler(handle.StringHandler)
                .BindByteHandler(handle.ByteHandler)
                .BindDisconnectHandler(handle.DisconnectHandler)
                .BindExceptionHandler(handle.ExceptionHandler);

            if (option.IsReconnection)
                _session.BindDisconnectHandler(ReconnectionHandler);

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
                    _session.Reconnect(new(_option.Host, _option.Port));
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
        public ValueTask SendAsync(string route, object data)
        {
            var requetBytes = _socketSerialize.SerializeOfByte(new SocketHeadInfo(default, SocketMode.String, route));
            var dataBytes = _socketSerialize.SerializeOfByte(data);
            return _session.SendBytesAsync(BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);
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
            var request = new RequestEventListener();

            var requetBytes = _socketSerialize.SerializeOfByte(new SocketHeadInfo(request.Id, SocketMode.Request, route));
            var dataBytes = _socketSerialize.SerializeOfByte(data);
            await _session.SendBytesAsync(BitConverter.GetBytes(requetBytes.Length), requetBytes, dataBytes);

            var resData = await request.ResultAsync();
            try
            {
                return _socketSerialize.Deserialize<T>(resData);
            }
            catch (Exception ex)
            {
                throw new DdonSocketRequestException(resData, "响应结果反序列化失败", ex);
            }
        }


        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _session.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
