using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Ddon.Domain.Entities;

namespace Ddon.Domain.BaseObject.Aggregate
{
    public abstract class AuditAggregateRoot<TKey> : AggregateRoot, IAuditAggregateRoot, IEntity<TKey>, IAuditEntity, ISoftDelete
            where TKey : IEquatable<TKey>
    {
        [Key, NotNull, Column(Order = 0)] public TKey Id { get; set; } = default!;

        [Column(Order = 101), JsonIgnore] public DateTime CreationTime { get; set; }

        [Column(Order = 111), JsonIgnore] public DateTime? LastModificationTime { get; set; }

        [Column(Order = 191), JsonIgnore] public bool IsDeleted { get; set; }
    }
}
