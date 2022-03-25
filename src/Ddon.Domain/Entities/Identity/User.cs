using Ddon.Domain.Entities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Identity.Entities
{
    public class User<TKey> : Entity<TKey>, IMultTenant<TKey> where TKey : IEquatable<TKey>
    {
        [AllowNull]
        public TKey TenantId { get; set; }

        [AllowNull]
        public virtual string UserName { get; set; }

        public virtual string? NormalizedUserName { get; set; }

        public virtual string? Email { get; set; }

        public virtual string? NormalizedEmail { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        [AllowNull]
        public virtual string PasswordHash { get; set; }

        public virtual string? SecurityStamp { get; set; }

        public virtual string? ConcurrencyStamp { get; set; }

        public virtual string? PhoneNumber { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        public virtual bool TwoFactorEnabled { get; set; }

        public virtual DateTimeOffset? LockoutEnd { get; set; }

        public virtual bool LockoutEnabled { get; set; }

        public virtual int AccessFailedCount { get; set; }

    }
}
