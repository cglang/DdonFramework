using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 常规自定义管道
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public class GeneralCustomPipeline<TContext> : IGeneralCustomPipeline<TContext>
    {
        private readonly PipelineDelegate<TContext> _delegate;

        public GeneralCustomPipeline(PipelineDelegate<TContext> @delegate)
        {
            _delegate = @delegate;
        }

        public Task ExecuteAsync(TContext context)
        {
            return _delegate(context);
        }
    }
}


