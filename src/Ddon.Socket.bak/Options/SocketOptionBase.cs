using System;
using Ddon.Socket.Session.Pipeline;

namespace Ddon.Socket.Options
{
    public class SocketOptionBase
    {
        public Action<ISocketMiddlewarePipelineRegistrar>? PipelineRegistrar { get; set; }

        public void ConfigureMiddlewares(Action<ISocketMiddlewarePipelineRegistrar> pipelineRegistrar)
        {
            PipelineRegistrar = pipelineRegistrar;
        }
    }
}
