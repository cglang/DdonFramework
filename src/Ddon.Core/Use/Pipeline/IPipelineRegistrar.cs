using System.Collections.Generic;
using System;

namespace Ddon.Core.Use.Pipeline
{
    public interface IPipelineRegistrar<T> : IEnumerator<IGeneralPipeline<T>>
    {
        void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralPipeline<T>;

        void AddMiddleware(Action<T> actionExecuting);

        void AddMiddleware(Action<T> actionExecuting, Action<T> actionExecuted);
    }
}
