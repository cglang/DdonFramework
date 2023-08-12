using System;
using System.Collections;
using System.Collections.Generic;

namespace Ddon.Core.Use.Pipeline
{
    public class PipelineRegistrar<T> : IPipelineRegistrar<T>
    {
        private readonly List<IGeneralPipeline<T>> _middlewareInstances;
        private readonly IPipelineInstanceProvider<T> _instanceProvider;

        private IGeneralPipeline<T>? curBox;
        private int curIndex;

        public PipelineRegistrar(IPipelineInstanceProvider<T> instanceProvider)
        {
            _instanceProvider = instanceProvider;
            _middlewareInstances = new();
            curIndex = -1;
        }

        public IGeneralPipeline<T> Current => curBox ?? throw new ArgumentNullException();

        object? IEnumerator.Current => Current;

        public void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralPipeline<T>
        {
            AddMiddleware(_instanceProvider.GetInstance(typeof(TMiddleware)));
        }

        public void AddMiddleware(Action<T> actionExecuting)
        {
            var defaultGeneralMiddleware = new DefaultGeneralPipeline<T>(actionExecuting);
            AddMiddleware(defaultGeneralMiddleware);
        }

        public void AddMiddleware(Action<T> actionExecuting, Action<T> actionExecuted)
        {
            var defaultGeneralMiddleware = new DefaultGeneralPipeline<T>(actionExecuting, actionExecuted);
            AddMiddleware(defaultGeneralMiddleware);
        }

        private void AddMiddleware(IGeneralPipeline<T> middleware)
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
