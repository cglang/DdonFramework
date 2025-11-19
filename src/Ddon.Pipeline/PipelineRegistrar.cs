using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 管道注册
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class PipelineRegistrar<TContext> : IPipelineRegistrar<TContext>
    {
        private readonly List<IGeneralPipelineMiddleware<TContext>> _middlewareInstances;
        private readonly IPipelineInstanceProvider<TContext> _instanceProvider;

        private int _curIndex;

        public PipelineRegistrar(IPipelineInstanceProvider<TContext> instanceProvider)
        {
            _instanceProvider = instanceProvider;
            _middlewareInstances = new List<IGeneralPipelineMiddleware<TContext>>();
            _curIndex = -1;
        }

        public IGeneralPipelineMiddleware<TContext> Current { get => _middlewareInstances[_curIndex]; }

        object IEnumerator.Current => Current;

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        public void AddMiddleware<TMiddleware>() where TMiddleware : IGeneralPipelineMiddleware<TContext>
        {
            AddMiddleware(_instanceProvider.GetInstance(typeof(TMiddleware)));
        }

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        public void AddMiddleware(Func<TContext, Task> actionExecuting)
        {
            var defaultGeneralMiddleware = new DefaultGeneralPipelineMiddleware<TContext>(actionExecuting);
            AddMiddleware(defaultGeneralMiddleware);
        }

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        public void AddMiddleware(Func<TContext, Task> actionExecuting, Func<TContext, Task> actionExecuted)
        {
            var defaultGeneralMiddleware = new DefaultGeneralPipelineMiddleware<TContext>(actionExecuting, actionExecuted);
            AddMiddleware(defaultGeneralMiddleware);
        }

        /// <summary>
        /// 添加管道中间件
        /// </summary>
        private void AddMiddleware(IGeneralPipelineMiddleware<TContext> middleware)
        {
            middleware.Index = _middlewareInstances.Count + 1;

            _middlewareInstances.Add(middleware);

            _curIndex = _middlewareInstances.Count;
        }

        public bool MoveNext()
        {
            if (--_curIndex < 0)
                return false;
            return true;
        }

        public void Reset()
        {
            _curIndex = _middlewareInstances.Count;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
