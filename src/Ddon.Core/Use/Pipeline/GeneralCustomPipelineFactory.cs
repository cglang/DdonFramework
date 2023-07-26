using System;

namespace Ddon.Core.Use.Pipeline
{
    public class GeneralCustomPipelineFactory<T>
    {
        public static GeneralCustomPipelineBuild<T> CreatePipelineBuild()
        {
            return new GeneralCustomPipelineBuild<T>();
        }
    }

    public class GeneralCustomPipelineBuild<T>
    {
        Action<MiddlewarePipelineRegistrar<T>>? _pipelineRegistrar;

        public GeneralCustomPipelineBuild<T> ConfigureMiddlewares(Action<MiddlewarePipelineRegistrar<T>> pipelineRegistrar)
        {
            _pipelineRegistrar = pipelineRegistrar;
            return this;
        }

        public GeneralCustomPipeline<T> Build()
        {
            var instanceProvider = new DefaultMiddlewareInstanceProvider<T>();
            var pipelineRegistrar = new MiddlewarePipelineRegistrar<T>(instanceProvider);

            _pipelineRegistrar?.Invoke(pipelineRegistrar);

            return new GeneralCustomPipeline<T>(pipelineRegistrar);
        }
    }
}
