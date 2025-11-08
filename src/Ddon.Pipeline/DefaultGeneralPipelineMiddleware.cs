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
        private readonly Func<TContext, Task> _actionExecuting;

        private readonly Func<TContext, Task> _actionExecuted;

        public DefaultGeneralPipelineMiddleware(Func<TContext, Task> actionExecuting)
        {
            _actionExecuting = actionExecuting;
        }

        public DefaultGeneralPipelineMiddleware(Func<TContext, Task> actionExecuting, Func<TContext, Task> actionExecuted)
        {
            _actionExecuting = actionExecuting;
            _actionExecuted = actionExecuted;
        }

        public async Task InvokeAsync(TContext context, PipelineDelegate<TContext> next)
        {
            await _actionExecuting.Invoke(context);

            await next(context);

            if (_actionExecuted != null)
                await _actionExecuted.Invoke(context);
        }
    }
}
