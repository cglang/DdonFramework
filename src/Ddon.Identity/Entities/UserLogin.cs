using System;

namespace Ddon.Domain.Entities.Identity
{
    public class UserLogin<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        public UserLogin(string loginProvider, string providerKey, TKey userId)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            UserId = userId;
        }

        public virtual string LoginProvider { get; set; }

        public virtual string ProviderKey { get; set; }

        public virtual string? ProviderDisplayName { get; set; }

        public virtual TKey UserId { get; set; }
    }
}
