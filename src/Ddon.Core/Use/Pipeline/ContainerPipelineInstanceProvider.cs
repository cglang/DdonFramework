using System;

namespace Ddon.Core.Use.Pipeline
{
    public class ContainerPipelineInstanceProvider<T> : IPipelineInstanceProvider<T>
    {
        private readonly IServiceProvider _service;

        public ContainerPipelineInstanceProvider(IServiceProvider service)
        {
            _service = service;
        }

        public IGeneralPipeline<T> GetInstance(Type type)
        {
            var instance = _service.GetService(type);
            if (instance is IGeneralPipeline<T> feneralMiddleware)
                return feneralMiddleware;
            throw new Exception();
        }
    }
}
