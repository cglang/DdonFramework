using System;

namespace Ddon.Core.Use.Pipeline
{
    public class DefaultPipelineInstanceProvider<T> : IPipelineInstanceProvider<T>
    {
        public IGeneralPipeline<T> GetInstance(Type type)
        {
            var instance = Activator.CreateInstance(type) ?? throw new Exception();
            if (instance is IGeneralPipeline<T> feneralMiddleware)
                return feneralMiddleware;
            throw new Exception();
        }
    }
}
