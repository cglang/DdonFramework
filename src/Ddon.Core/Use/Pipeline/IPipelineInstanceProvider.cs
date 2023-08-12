using System;

namespace Ddon.Core.Use.Pipeline
{
    public interface IPipelineInstanceProvider<T>
    {
        IGeneralPipeline<T> GetInstance(Type type);
    }
}
