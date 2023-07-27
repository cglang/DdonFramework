using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Ddon.Socket.Session.Pipeline;

namespace Ddon.Socket.Options
{
    public class SocketServerOption
    {
        [AllowNull, NotNull]
        public IPEndPoint IPEndPoint { get; set; }

        public Action<ISocketMiddlewarePipelineRegistrar>? PipelineRegistrar { get; set; }

        public void ConfigureMiddlewares(Action<ISocketMiddlewarePipelineRegistrar> pipelineRegistrar)
        {
            PipelineRegistrar = pipelineRegistrar;
        }
    }
}
