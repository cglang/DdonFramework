using System;
using System.Threading.Tasks;
using Ddon.Core.Services.IdWorker;
using Ddon.Core.Use.Pipeline;
using Ddon.Socket.Session.Pipeline;

namespace Ddon.Socket.Session.Middleware
{
    public class TestMiddleware : ISocketMiddleware
    {
        private readonly IIdGenerator idGenerator;

        public TestMiddleware(IIdGenerator idGenerator)
        {
            this.idGenerator = idGenerator;
        }

        public async Task InvokeAsync(SocketContext context, MiddlewareDelegate<SocketContext> next)
        {
            Console.WriteLine($"管道测试:{idGenerator.GetHashCode()}:{idGenerator.CreateId()}");

            await next(context);
        }
    }
}
