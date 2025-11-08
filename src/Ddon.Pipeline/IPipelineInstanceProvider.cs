using System;

namespace Ddon.Pipeline
{
    /// <summary>
    /// 管道实例提供器接口
    /// </summary>
    /// <typeparam name="TContext">管道上下文数据</typeparam>
    public interface IPipelineInstanceProvider<T>
    {
        IGeneralPipelineMiddleware<T> GetInstance(Type type);
    }
}
