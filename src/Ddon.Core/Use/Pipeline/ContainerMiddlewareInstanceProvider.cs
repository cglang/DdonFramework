using System;

namespace Ddon.Core.Use.Pipeline
{
    public class ContainerMiddlewareInstanceProvider<T> : IMiddlewareInstanceProvider<T>
    {
        private readonly IServiceProvider _service;

        public ContainerMiddlewareInstanceProvider(IServiceProvider service)
        {
            _service = service;
        }

        public IGeneralMiddleware<T> GetInstance(Type type)
        {
            var instance = _service.GetService(type);
            if (instance is IGeneralMiddleware<T> feneralMiddleware)
                return feneralMiddleware;
            throw new Exception();
        }
    }
}
