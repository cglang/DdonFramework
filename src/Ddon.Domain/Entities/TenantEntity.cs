using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Domain.Entities
{
    public abstract class TenantEntity<TKey> : Entity<TKey>, IMultTenant<TKey>
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1100)]
        [AllowNull]
        public TKey TenantId { get; set; }
    }
}
