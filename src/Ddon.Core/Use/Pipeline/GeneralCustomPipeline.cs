using System.Threading.Tasks;

namespace Ddon.Core.Use.Pipeline
{

    public class GeneralCustomPipeline<T> : IGeneralCustomPipeline<T>
    {
        private readonly IMiddlewarePipelineRegistrar<T> _pipelineRegistrar;

        public GeneralCustomPipeline(IMiddlewarePipelineRegistrar<T> pipelineRegistrar)
        {
            _pipelineRegistrar = pipelineRegistrar;
        }

        public async Task ExecuteAsync(T context)
        {
            static Task FinalMiddleware(T ctx) => Task.CompletedTask;

            MiddlewareDelegate<T> pipeline = FinalMiddleware;

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
