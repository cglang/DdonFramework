using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 管道注册接口 也用于注册的管道存储
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPipelineRegistrar<T> : IEnumerator<IGeneralPipelineMiddleware<T>>
    {
        void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralPipelineMiddleware<T>;

        void AddMiddleware(Func<T, Task> actionExecuting);

        void AddMiddleware(Func<T, Task> actionExecuting, Func<T, Task> actionExecuted);
    }
}
