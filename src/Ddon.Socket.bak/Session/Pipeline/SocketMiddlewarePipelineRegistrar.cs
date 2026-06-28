using Ddon.Core.Use.Pipeline;

namespace Ddon.Socket.Session.Pipeline
{
    public interface ISocketMiddlewarePipelineRegistrar : IPipelineRegistrar<SocketContext>
    {
    }

    public class SocketMiddlewarePipelineRegistrar : PipelineRegistrar<SocketContext>, ISocketMiddlewarePipelineRegistrar
    {
        public SocketMiddlewarePipelineRegistrar(IPipelineInstanceProvider<SocketContext> instanceProvider) : base(instanceProvider)
        {
        }
    }
}
