using System;
using System.Threading.Tasks;
using Ddon.Core.Use.Pipeline;
using Ddon.Socket.Session.Pipeline;

namespace Ddon.Socket.Session.Middleware
{
    public class TestMiddleware : ISocketMiddleware
    {
        public async Task InvokeAsync(SocketContext context, MiddlewareDelegate<SocketContext> next)
        {
            Console.WriteLine("测试测试");

            await next(context);
        }
    }
}
