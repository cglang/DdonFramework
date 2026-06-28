using Ddon.Pipeline;
using Ddon.Socket.Models;

namespace Ddon.Socket.Abstractions
{
    public interface ISocketMiddleware : IGeneralPipelineMiddleware<SocketContext>
    {
    }
}
