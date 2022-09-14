using Ddon.Core.Exceptions;
using Ddon.Core.Services.LazyService.Static;
using Ddon.Core.Use;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Route;
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
        private ILogger Logger => ServiceProvider.GetRequiredService<ILogger>();

        protected readonly SocketSession session;


        protected SocketClient(string host, int post)
        {
            var tcpClient = new TcpClient(host, post);
            session = new SocketSession(tcpClient, ExceptionHandler);
        }

        public static SocketSession CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<TDdonSocketRouteMapLoadBase>();
            var t = new SocketClient<TDdonSocketRouteMapLoadBase>(host, post);
            return t.session;
        }

        private Func<DdonSocketCore, DdonSocketException, Task> ExceptionHandler => async (conn, ex) =>
        {
            //Console.WriteLine("Scoket 异常");
            Logger?.LogError(ex, "Scoket 异常");
            await Task.CompletedTask;
        };
    }

    public class SocketClient : SocketClient<DeafultDdonSocketRouteMap>
    {
        protected SocketClient(string host, int post) : base(host, post)
        {
        }

        public new static SocketSession CreateClient(IServiceProvider serviceProvider, string host, int post)
        {
            LazyServiceProvider.InitServiceProvider(serviceProvider);
            DdonSocketRouteMap.Init<DeafultDdonSocketRouteMap>();
            var t = new SocketClient(host, post);
            return t.session;
        }
    }
}