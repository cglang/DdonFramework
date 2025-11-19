using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 管道注册接口 也用于注册的管道存储
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IPipelineRegistrar<TContext> : IEnumerator<IGeneralPipelineMiddleware<TContext>>
    {
        void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralPipelineMiddleware<TContext>;

        void AddMiddleware(Func<TContext, Task> actionExecuting);

        void AddMiddleware(Func<TContext, Task> actionExecuting, Func<TContext, Task> actionExecuted);
    }
}
