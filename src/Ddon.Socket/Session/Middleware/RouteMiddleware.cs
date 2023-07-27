using System.Threading.Tasks;
using Ddon.Core.Use.Pipeline;
using Ddon.Socket.Session.Pipeline;
using Ddon.Socket.Session.Route;

namespace Ddon.Socket.Session.Middleware
{
    public class RouteMiddleware : ISocketMiddleware
    {
        public RouteMiddleware() { }

        public async Task InvokeAsync(SocketContext context, MiddlewareDelegate<SocketContext> next)
        {
            context.SetEndPoint(SocketRouteMap.Get(context.Head.Route));

            await next(context);
        }
    }
}
