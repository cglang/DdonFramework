using Ddon.Core.Use.Pipeline;

namespace Ddon.Socket.Session.Pipeline
{
    public interface ISocketByteCustomPipeline : IGeneralCustomPipeline<SocketContext>
    {
    }

    public class SocketByteCustomPipeline : GeneralCustomPipeline<SocketContext>, ISocketByteCustomPipeline
    {
        public SocketByteCustomPipeline(ISocketMiddlewarePipelineRegistrar pipelineRegistrar) : base(pipelineRegistrar)
        {
        }
    }
}
