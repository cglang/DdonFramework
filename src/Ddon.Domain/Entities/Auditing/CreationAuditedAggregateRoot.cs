using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddon.Domain.Entities.Auditing
{
    [Serializable]
    public abstract class CreationAuditedAggregateRoot<TKey, TAuditKey> : AggregateRoot<TKey>, ICreationAuditedObject<TKey, TAuditKey>
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1001)]
        public virtual DateTime CreationTime { get; set; }

        [Column(Order = 1002)]
        public virtual TAuditKey? CreatorId { get; set; }
    }

    [Serializable]
    public abstract class CreationAuditedAggregateRoot<TKey, TAuditKey, TUser> : CreationAuditedAggregateRoot<TKey, TAuditKey>, ICreationAuditedObject<TKey, TAuditKey, TUser>
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1003)]
        public virtual TUser? Creator { get; set; }
    }
}
