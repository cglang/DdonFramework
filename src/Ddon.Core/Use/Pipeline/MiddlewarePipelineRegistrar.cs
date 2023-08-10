using System;
using System.Collections;
using System.Collections.Generic;

namespace Ddon.Core.Use.Pipeline
{
    public class MiddlewarePipelineRegistrar<T> : IMiddlewarePipelineRegistrar<T>
    {
        private readonly List<IGeneralMiddleware<T>> _middlewareInstances;
        private readonly IMiddlewareInstanceProvider<T> _instanceProvider;

        private IGeneralMiddleware<T>? curBox;
        private int curIndex;

        public MiddlewarePipelineRegistrar(IMiddlewareInstanceProvider<T> instanceProvider)
        {
            _instanceProvider = instanceProvider;
            _middlewareInstances = new();
            curIndex = -1;
        }

        public IGeneralMiddleware<T> Current => curBox ?? throw new ArgumentNullException();

        object? IEnumerator.Current => Current;

        public void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralMiddleware<T>
        {
            AddMiddleware(_instanceProvider.GetInstance(typeof(TMiddleware)));
        }

        public void AddMiddleware(Action<T> actionExecuting)
        {
            var defaultGeneralMiddleware = new DefaultGeneralMiddleware<T>(actionExecuting);
            AddMiddleware(defaultGeneralMiddleware);
        }

        public void AddMiddleware(Action<T> actionExecuting, Action<T> actionExecuted)
        {
            var defaultGeneralMiddleware = new DefaultGeneralMiddleware<T>(actionExecuting, actionExecuted);
            AddMiddleware(defaultGeneralMiddleware);
        }

        private void AddMiddleware(IGeneralMiddleware<T> middleware)
        {
            _middlewareInstances.Add(middleware);
            curIndex = _middlewareInstances.Count;
        }

        public bool MoveNext()
        {
            if (--curIndex < 0)
                return false;
            else
                curBox = _middlewareInstances[curIndex];
            return true;
        }

        public void Reset()
        {
            curIndex = _middlewareInstances.Count;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
