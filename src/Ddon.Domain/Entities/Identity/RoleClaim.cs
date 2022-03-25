using Ddon.Domain.Entities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Identity.Entities
{
    public class RoleClaim<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        [AllowNull]
        public TKey RoleId { get; set; }

        [AllowNull]
        public string ClaimType { get; set; }

        [AllowNull]
        public string ClaimValue { get; set; }
    }
}
