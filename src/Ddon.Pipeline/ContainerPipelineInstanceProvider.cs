using System;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 容器管道实例提供器
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public class ContainerPipelineInstanceProvider<TContext> : IPipelineInstanceProvider<TContext>
    {
        private readonly IServiceProvider _service;

        public ContainerPipelineInstanceProvider(IServiceProvider service)
        {
            _service = service;
        }

        public IGeneralPipelineMiddleware<TContext> GetInstance(Type type)
        {
            var instance = _service.GetService(type);
            if (instance is IGeneralPipelineMiddleware<TContext> feneralMiddleware)
                return feneralMiddleware;
            throw new Exception();
        }
    }
}
