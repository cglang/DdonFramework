using System;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 常规自定义管道工厂
    /// </summary>
    /// <typeparam name="T">管道上下文数据</typeparam>
    public class GeneralCustomPipelineFactory<T>
    {
        public static GeneralCustomPipelineBuild<T> CreatePipelineBuild()
        {
            return new GeneralCustomPipelineBuild<T>();
        }
    }

    /// <summary>
    /// 常规自定义管道构造器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GeneralCustomPipelineBuild<T>
    {
        Action<PipelineRegistrar<T>> _pipelineRegistrar;

        public GeneralCustomPipelineBuild<T> ConfigureMiddlewares(Action<PipelineRegistrar<T>> pipelineRegistrar)
        {
            _pipelineRegistrar = pipelineRegistrar;
            return this;
        }

        public GeneralCustomPipeline<T> Build()
        {
            var instanceProvider = new DefaultPipelineInstanceProvider<T>();
            var pipelineRegistrar = new PipelineRegistrar<T>(instanceProvider);

            _pipelineRegistrar?.Invoke(pipelineRegistrar);

            return new GeneralCustomPipeline<T>(pipelineRegistrar);
        }
    }
}
