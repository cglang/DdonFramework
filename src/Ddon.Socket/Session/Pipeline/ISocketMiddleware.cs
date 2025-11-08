using Ddon.Core.Use.Pipeline;

namespace Ddon.Socket.Session.Pipeline
{
    public interface ISocketMiddleware : IGeneralPipelineMiddleware<SocketContext>
    {
    }
}
