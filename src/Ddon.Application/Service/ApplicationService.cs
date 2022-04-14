using Ddon.Core.Services.Guids;
using Ddon.Core.Services.LazyService;
using System;

namespace Ddon.Application.Service
{
    public class ApplicationService<TKey> where TKey : IEquatable<TKey>
    {
        protected ILazyServiceProvider LazyServiceProvider { get; }

        /// <summary>
        /// 连续 Guid 生成
        /// </summary>
        protected IGuidGenerator GuidGenerator => LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>();

        public ApplicationService(ILazyServiceProvider lazyServiceProvider)
        {
            LazyServiceProvider = lazyServiceProvider;
        }
    }
}
