using Ddon.Core.Services.LazyService;
using System;

namespace Ddon.Application.Service
{
    public class ApplicationService<TKey> where TKey : IEquatable<TKey>
    {
        protected ILazyServiceProvider LazyServiceProvider { get; }

        public ApplicationService(ILazyServiceProvider lazyServiceProvider)
        {
            LazyServiceProvider = lazyServiceProvider;
        }
    }
}
