using System;

namespace Ddon.Core.Use.Pipeline
{
    public interface IMiddlewareInstanceProvider<T>
    {
        IGeneralMiddleware<T> GetInstance(Type type);
    }
}
