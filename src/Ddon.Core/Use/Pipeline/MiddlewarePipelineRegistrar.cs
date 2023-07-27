using System;
using System.Collections;
using System.Collections.Generic;

namespace Ddon.Core.Use.Pipeline
{
    public class MiddlewarePipelineRegistrar<T> : IMiddlewarePipelineRegistrar<T>
    {
        private readonly List<Type> _middlewares;
        private readonly IMiddlewareInstanceProvider<T> _instanceProvider;

        private IGeneralMiddleware<T>? curBox;
        private int curIndex;

        public MiddlewarePipelineRegistrar(IMiddlewareInstanceProvider<T> instanceProvider)
        {
            _instanceProvider = instanceProvider;
            _middlewares = new();
            curIndex = -1;
        }

        public IGeneralMiddleware<T> Current => curBox ?? throw new ArgumentNullException();

        object? IEnumerator.Current => Current;

        public void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralMiddleware<T>
        {
            _middlewares.Add(typeof(TMiddleware));
            curIndex = _middlewares.Count;
        }

        public bool MoveNext()
        {
            if (--curIndex < 0)
                return false;
            else
                curBox = _instanceProvider.GetInstance(_middlewares[curIndex]);
            return true;
        }

        public void Reset()
        {
            curIndex = _middlewares.Count;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
