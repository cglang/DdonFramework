using System;

namespace Ddon.Core.Use.Pipeline
{
    public class DefaultMiddlewareInstanceProvider<T> : IMiddlewareInstanceProvider<T>
    {
        public IGeneralMiddleware<T> GetInstance(Type type)
        {
            var instance = Activator.CreateInstance(type) ?? throw new Exception();
            if (instance is IGeneralMiddleware<T> feneralMiddleware)
                return feneralMiddleware;
            throw new Exception();
        }
    }
}
