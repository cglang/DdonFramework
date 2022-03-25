using Ddon.Domain.Entities;
using System;

namespace Ddon.Identity.Entities
{
    public class UserToken<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        public UserToken(TKey userId, string loginProvider, string name)
        {
            UserId = userId;
            LoginProvider = loginProvider;
            Name = name;
        }

        public virtual TKey UserId { get; set; }

        public virtual string LoginProvider { get; set; }

        public virtual string Name { get; set; }

        public virtual string? Value { get; set; }
    }
}
