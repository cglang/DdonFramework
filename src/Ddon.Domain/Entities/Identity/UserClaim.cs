using Ddon.Domain.Entities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Identity.Entities
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
