using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddon.Domain.Entities.Auditing
{
    [Serializable]
    public abstract class AuditedAggregateRoot<TKey, TAuditKey> : CreationAuditedAggregateRoot<TKey, TAuditKey>, IAuditedObject<TKey, TAuditKey>
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1004)]
        public virtual DateTime? LastModificationTime { get; set; }

        [Column(Order = 1005)]
        public virtual TAuditKey? LastModifierId { get; set; }
    }

    [Serializable]
    public abstract class AuditedAggregateRoot<TKey, TAuditKey, TUser> : AuditedAggregateRoot<TKey, TAuditKey>, IAuditedObject<TKey, TAuditKey, TUser>
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1003)]
        public TUser? Creator { get; set; }

        [Column(Order = 1006)]
        public TUser? Modifier { get; set; }
    }
}