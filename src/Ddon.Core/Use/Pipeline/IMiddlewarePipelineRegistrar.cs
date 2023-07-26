using System.Collections.Generic;

namespace Ddon.Core.Use.Pipeline
{
    public interface IMiddlewarePipelineRegistrar<T> : IEnumerator<IGeneralMiddleware<T>>
    {
        void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralMiddleware<T>, new();
    }
}
