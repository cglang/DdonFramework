using System;
using System.Threading.Tasks;
using Ddon.Socket.Core;
using Ddon.Socket.Options;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Handler;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket
{
    public class SocketClient : SocketCoreSession
    {
        private readonly SocketClientOption _option;
        private readonly ILogger<SocketClient> _logger;

        public SocketClient(SocketClientOption option, SocketSessionHandler handle, ILogger<SocketClient> logger) : base(option.Host, option.Port)
        {
            BindStringHandler(handle.StringHandler);
            BindByteHandler(handle.ByteHandler);
            BindDisconnectHandler(handle.DisconnectHandler);
            BindExceptionHandler(handle.ExceptionHandler);

            if (option.IsReconnection)
                BindDisconnectHandler(ReconnectionHandler);

            _option = option;
            _logger = logger;

            DdonSocketResponsePool.Start();
        }

        private Func<SocketCoreSession, Task> ReconnectionHandler => async session =>
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
    }
}
