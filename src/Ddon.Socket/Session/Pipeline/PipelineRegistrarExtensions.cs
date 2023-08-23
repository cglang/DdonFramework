using Ddon.Socket.Session.Middleware;
using Ddon.Socket.Session.Pipeline;

namespace Ddon.Socket
{
    public static class PipelineRegistrarExtensions
    {
        public static void UseEndPoints(this ISocketMiddlewarePipelineRegistrar pipelineRegistrar)
        {
            pipelineRegistrar.AddMiddleware<RouteMiddleware>();
            pipelineRegistrar.AddMiddleware<EndPointMiddleware>();
        }
    }
}
