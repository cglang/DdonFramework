using System;
using System.Collections;
using System.Collections.Generic;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 管道注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PipelineRegistrar<T> : IPipelineRegistrar<T>
    {
        private readonly List<IGeneralPipelineMiddleware<T>> _middlewareInstances;
        private readonly IPipelineInstanceProvider<T> _instanceProvider;

        private IGeneralPipelineMiddleware<T> curBox;
        private int curIndex;

        public PipelineRegistrar(IPipelineInstanceProvider<T> instanceProvider)
        {
            _instanceProvider = instanceProvider;
            _middlewareInstances = new List<IGeneralPipelineMiddleware<T>>();
            curIndex = -1;
        }

        public IGeneralPipelineMiddleware<T> Current => curBox ?? throw new ArgumentNullException();

        object IEnumerator.Current => Current;

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        public void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralPipelineMiddleware<T>
        {
            AddMiddleware(_instanceProvider.GetInstance(typeof(TMiddleware)));
        }

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        public void AddMiddleware(Action<T> actionExecuting)
        {
            var defaultGeneralMiddleware = new DefaultGeneralPipelineMiddleware<T>(actionExecuting);
            AddMiddleware(defaultGeneralMiddleware);
        }

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        public void AddMiddleware(Action<T> actionExecuting, Action<T> actionExecuted)
        {
            var defaultGeneralMiddleware = new DefaultGeneralPipelineMiddleware<T>(actionExecuting, actionExecuted);
            AddMiddleware(defaultGeneralMiddleware);
        }

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        private void AddMiddleware(IGeneralPipelineMiddleware<T> middleware)
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
