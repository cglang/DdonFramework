using System.Collections.Generic;
using System;

namespace Ddon.Core.Use.Pipeline
{
    public interface IMiddlewarePipelineRegistrar<T> : IEnumerator<IGeneralMiddleware<T>>
    {
        void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralMiddleware<T>;

        void AddMiddleware(Action<T> actionExecuting);

        void AddMiddleware(Action<T> actionExecuting, Action<T> actionExecuted);
    }
}
