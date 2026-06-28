using System.Threading.Tasks;
using Ddon.Pipeline;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Models;

namespace Ddon.Socket.Core
{
    public class SocketPipeline : ISocketPipeline
    {
        private readonly IGeneralCustomPipeline<SocketContext> _pipeline;

        public SocketPipeline(IGeneralCustomPipeline<SocketContext> pipeline)
        {
            _pipeline = pipeline;
        }

        public Task ExecuteAsync(SocketContext context)
        {
            return _pipeline.ExecuteAsync(context);
        }
    }
}
