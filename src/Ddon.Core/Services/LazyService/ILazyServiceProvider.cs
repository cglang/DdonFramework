using System;
using Ddon.DependencyInjection;

namespace Ddon.Core.Services.LazyService
{
    public interface ILazyServiceProvider : ITransientDependency
    {
        T LazyGetRequiredService<T>();

        object LazyGetRequiredService(Type serviceType);

        T LazyGetService<T>();

        object LazyGetService(Type serviceType);

        T LazyGetService<T>(T defaultValue);

        object LazyGetService(Type serviceType, object defaultValue);

        object LazyGetService(Type serviceType, Func<IServiceProvider, object> factory);

        T LazyGetService<T>(Func<IServiceProvider, object> factory);

        IServiceProvider ServiceProvider { get; set; }
    }
}
