using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddon.Domain.Entities.Auditing
{
    [Serializable, Index(nameof(IsDeleted))]
    public abstract class FullAuditedAggregateRoot<TKey> : AuditedAggregateRoot<TKey, TKey>, IFullAuditedObject
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 9999)]
        public bool IsDeleted { get; set; }
    }

    [Serializable, Index(nameof(IsDeleted))]
    public abstract class FullAuditedAggregateRoot<TKey, TAuditKey> : AuditedAggregateRoot<TKey, TAuditKey>, IFullAuditedObject<TKey, TAuditKey>
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1007)]
        public virtual TKey? DeleterId { get; set; }

        [Column(Order = 9999)]
        public bool IsDeleted { get; set; }
    }

    [Serializable, Index(nameof(IsDeleted))]
    public abstract class FullAuditedAggregateRoot<TKey, TAuditKey, TUser> : FullAuditedAggregateRoot<TKey, TAuditKey>, IFullAuditedObject<TKey, TAuditKey, TUser>
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1003)]
        public TUser? Creator { get; set; }

        [Column(Order = 1006)]
        public TUser? Modifier { get; set; }

        [Column(Order = 1008)]
        public TUser? Deleter { get; set; }
    }
}