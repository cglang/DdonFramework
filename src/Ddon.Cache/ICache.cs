using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;

namespace Ddon.Cache
{
    public interface ICache
    {
        object? Get(object key);

        TItem? Get<TItem>(object key);

        TItem Set<TItem>(object key, TItem value);

        TItem Set<TItem>(object key, TItem value, MemoryCacheEntryOptions options);

        TItem Set<TItem>(object key, TItem value, IChangeToken expirationToken);

        TItem Set<TItem>(object key, TItem value, DateTimeOffset absoluteExpiration);

        TItem Set<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow);

        bool ContainsKey(object key);

        void Remove(object key);

        void Remove(object[] keys);
    }
}
