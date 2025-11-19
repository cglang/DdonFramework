using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 常规自定义管道工厂
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public class GeneralCustomPipelineFactory<TContext>
    {
        public static GeneralCustomPipelineBuild<TContext> CreatePipelineBuild()
        {
            return new GeneralCustomPipelineBuild<TContext>();
        }
    }

    /// <summary>
    /// 常规自定义管道构造器
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class GeneralCustomPipelineBuild<TContext>
    {
        List<Action<PipelineRegistrar<TContext>>> _pipelineRegistrars;

        public GeneralCustomPipelineBuild()
        {
            _pipelineRegistrars = new List<Action<PipelineRegistrar<TContext>>>();
        }

        public GeneralCustomPipelineBuild<TContext> ConfigureMiddlewares(Action<PipelineRegistrar<TContext>> pipelineRegistrar)
        {
            _pipelineRegistrars.Add(pipelineRegistrar);
            return this;
        }

        public GeneralCustomPipeline<TContext> Build()
        {
            var instanceProvider = new DefaultPipelineInstanceProvider<TContext>();
            var pipelineRegistrar = new PipelineRegistrar<TContext>(instanceProvider);

            foreach (var registrar in _pipelineRegistrars)
                registrar.Invoke(pipelineRegistrar);

            PipelineDelegate<TContext> pipelineDelegate = FinalMiddleware;

            pipelineRegistrar.Reset();
            while (pipelineRegistrar.MoveNext())
            {
                var currentMiddleware = pipelineRegistrar.Current;
                var nextMiddleware = pipelineDelegate;
                pipelineDelegate = (ctx) => currentMiddleware.InvokeAsync(ctx, nextMiddleware);
            }

            return new GeneralCustomPipeline<TContext>(pipelineDelegate);
        }

        static Task FinalMiddleware(TContext ctx) => Task.CompletedTask;
    }
}
