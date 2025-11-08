using System;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 默认管道实例提供器
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public class DefaultPipelineInstanceProvider<TContext> : IPipelineInstanceProvider<TContext>
    {
        public IGeneralPipelineMiddleware<TContext> GetInstance(Type type)
        {
            var instance = Activator.CreateInstance(type) ?? throw new Exception();
            if (instance is IGeneralPipelineMiddleware<TContext> feneralMiddleware)
                return feneralMiddleware;
            throw new Exception();
        }
    }
}
