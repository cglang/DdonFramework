using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 常规自定义管道
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public class GeneralCustomPipeline<TContext> : IGeneralCustomPipeline<TContext>
    {
        private readonly IPipelineRegistrar<TContext> _pipelineRegistrar;

        public GeneralCustomPipeline(IPipelineRegistrar<TContext> pipelineRegistrar)
        {
            _pipelineRegistrar = pipelineRegistrar;
        }

        static Task FinalMiddleware(TContext ctx) => Task.CompletedTask;

        public async Task ExecuteAsync(TContext context)
        {

            PipelineDelegate<TContext> pipeline = FinalMiddleware;

            _pipelineRegistrar.Reset();
            while (_pipelineRegistrar.MoveNext())
            {
                var currentMiddleware = _pipelineRegistrar.Current;
                var nextMiddleware = pipeline;
                pipeline = (ctx) => currentMiddleware.InvokeAsync(ctx, nextMiddleware);
            }

            await pipeline(context);
        }
    }
}


