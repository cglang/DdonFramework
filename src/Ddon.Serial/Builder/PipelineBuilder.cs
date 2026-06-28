using System;
using System.Threading.Tasks;
using Ddon.Pipeline;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Models;

namespace Ddon.Serial.Builder
{
    public class PipelineBuilder
    {
        private readonly GeneralCustomPipelineBuild<SerialContext> _build;

        internal PipelineBuilder()
        {
            _build = GeneralCustomPipelineFactory<SerialContext>.CreatePipelineBuild();
        }

        public PipelineBuilder Use<TMiddleware>() where TMiddleware : ISerialMiddleware
        {
            _build.ConfigureMiddlewares(registrar =>
            {
                registrar.AddMiddleware<TMiddleware>();
            });
            return this;
        }

        public PipelineBuilder Use(Func<SerialContext, Task> action)
        {
            _build.ConfigureMiddlewares(registrar =>
            {
                registrar.AddMiddleware(action);
            });
            return this;
        }

        internal GeneralCustomPipeline<SerialContext> Build()
        {
            return _build.Build();
        }
    }
}
