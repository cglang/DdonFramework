using Ddon.Core.Use.Pipeline;

namespace Ddon.Socket.Session.Pipeline
{
    public interface ISocketMiddlewarePipelineRegistrar : IMiddlewarePipelineRegistrar<SocketContext>
    {
    }

    public class SocketMiddlewarePipelineRegistrar : MiddlewarePipelineRegistrar<SocketContext>, ISocketMiddlewarePipelineRegistrar
    {
        public SocketMiddlewarePipelineRegistrar(IMiddlewareInstanceProvider<SocketContext> instanceProvider) : base(instanceProvider)
        {
        }
    }
}
