using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddon.Domain.Entities.Auditing
{
    public abstract class AuditedAggregateRoot<TKey> : AggregateRoot<TKey>, IAuditedObject, ISoftDelete
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1001)]
        public virtual DateTime CreationTime { get; set; }

        [Column(Order = 1004)]
        public virtual DateTime? LastModificationTime { get; set; }

        [Column(Order = 9999)]
        public bool IsDeleted { get; set; }
    }
}