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
            var instance = Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"The service type {type.FullName} is not a IGeneralPipelineMiddleware<{typeof(TContext).Name}>.");

            if (instance is IGeneralPipelineMiddleware<TContext> generalMiddleware)
                return generalMiddleware;
            throw new Exception();
        }
    }
}
