using System;
using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 默认常规管道中间件 用于向管道中添加匿名中间件
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public class DefaultGeneralPipelineMiddleware<TContext> : IGeneralPipelineMiddleware<TContext>
    {
        private readonly Action<TContext> _actionExecuting;

        private readonly Action<TContext> _actionExecuted;

        public DefaultGeneralPipelineMiddleware(Action<TContext> actionExecuting)
        {
            _actionExecuting = actionExecuting;
        }

        public DefaultGeneralPipelineMiddleware(Action<TContext> actionExecuting, Action<TContext> actionExecuted)
        {
            _actionExecuting = actionExecuting;
            _actionExecuted = actionExecuted;
        }

        public async Task InvokeAsync(TContext context, PipelineDelegate<TContext> next)
        {
            _actionExecuting.Invoke(context);

            await next(context);

            _actionExecuted?.Invoke(context);
        }
    }
}
