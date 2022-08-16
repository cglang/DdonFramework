using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Domain.Entities.Identity
{
    public class UserClaim<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        [AllowNull]
        public virtual TKey UserId { get; set; }

        [AllowNull]
        public virtual string ClaimType { get; set; }

        [AllowNull]
        public virtual string ClaimValue { get; set; }
    }
}
