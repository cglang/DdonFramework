using Ddon.Core.Services.LazyService.Static;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Route;
using Ddon.TuuTools.Socket.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ddon.Socket
{
    public class SocketClient<TDdonSocketRouteMapLoadBase> where TDdonSocketRouteMapLoadBase : DdonSocketRouteMapLoadBase, new()
    {
        private IServiceProvider ServiceProvider => LazyServiceProvider.LazyServicePrivider.ServiceProvider;
        private ILogger Logger => ServiceProvider.GetRequiredService<ILogger<SocketClient<TDdonSocketRouteMapLoadBase>>>();

        protected SocketSession Session { get; set; }

        private readonly string _host;
        private readonly int _post;

        protected SocketClient(string host, int post, bool isReconnection)
        {
            _host = host;
            _post = post;

            if (isReconnection)
            {
                ExceptionHandler += ReconnectionHandler;
            }
            ExceptionHandler += DefaultExceptionHandler;

            Session = new SocketSession(new TcpClient(host, post), ExceptionHandler);
        }

        public static SocketSession CreateClient(IServiceProvider serviceProvider, string host, int post, bool isReconnection = true)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            var t = new SocketClient<TDdonSocketRouteMapLoadBase>(host, post, isReconnection);
            return t.Session;
        }

        private Func<SocketSession, DdonSocketException, Task> ExceptionHandler { get; }

        private Func<SocketSession, DdonSocketException, Task> DefaultExceptionHandler => async (conn, ex) =>
        {
            if (ex.InnerException is ObjectDisposedException)
            {
                Logger?.LogWarning(ex.InnerException, "远程连接已断开");
            }
            else
            {
                Logger?.LogError(ex, "Scoket 异常");
                await Task.CompletedTask;
            }
        };

        private Func<SocketSession, DdonSocketException, Task> ReconnectionHandler => async (a, b) =>
        {
            Session.Dispose();

            for (int number = 1; ; number++)
            {
                try
                {
                    Session = new SocketSession(new TcpClient(_host, _post), ExceptionHandler);
                    Logger?.LogInformation("断线重连成功,重试次数:{0}", number);
                    break;
                }
                catch (Exception ex)
                {
                    Logger?.LogWarning(ex, "正在尝试断线重连,已重试次数:{0}", number);
                    await Task.Delay(100);
                }
            }
        };
    }

    public class SocketClient : SocketClient<DeafultDdonSocketRouteMap>
    {
        protected SocketClient(string host, int post, bool isReconnection) : base(host, post, isReconnection) { }

        public new static SocketSession CreateClient(IServiceProvider serviceProvider, string host, int post, bool isReconnection = true)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();
            var t = new SocketClient(host, post, isReconnection);
            return t.Session;
        }
    }
}