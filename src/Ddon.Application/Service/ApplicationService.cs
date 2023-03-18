using Ddon.Core.Services.LazyService;
using System;

namespace Ddon.Application.Service
{
    public class ApplicationService<TKey> where TKey : IEquatable<TKey>
    {
        protected IServiceProvider ServiceProvider { get; }

        public ApplicationService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
