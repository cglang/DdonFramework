using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Domain.Entities.Identity
{
    public class PermissionGrant<TKey> : Entity<TKey>, IMultTenant<TKey> where TKey : IEquatable<TKey>
    {
        [AllowNull]
        public TKey TenantId { get; set; }

        [AllowNull]
        public string Name { get; set; }

        public PermissionGrantType Type { get; set; }

        [AllowNull]
        public TKey GrantKey { get; set; }
    }

    public enum PermissionGrantType
    {
        User = 1,
        Role = 2
    }
}
