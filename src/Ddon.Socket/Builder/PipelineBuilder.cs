using System;
using System.Threading.Tasks;
using Ddon.Pipeline;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Models;

namespace Ddon.Socket.Builder
{
    public class PipelineBuilder
    {
        private readonly GeneralCustomPipelineBuild<SocketContext> _build;

        internal PipelineBuilder()
        {
            _build = GeneralCustomPipelineFactory<SocketContext>.CreatePipelineBuild();
        }

        public PipelineBuilder Use<TMiddleware>() where TMiddleware : ISocketMiddleware
        {
            _build.ConfigureMiddlewares(registrar =>
            {
                registrar.AddMiddleware<TMiddleware>();
            });
            return this;
        }

        public PipelineBuilder Use(Func<SocketContext, Task> action)
        {
            _build.ConfigureMiddlewares(registrar =>
            {
                registrar.AddMiddleware(action);
            });
            return this;
        }

        internal GeneralCustomPipeline<SocketContext> Build()
        {
            return _build.Build();
        }
    }
}
