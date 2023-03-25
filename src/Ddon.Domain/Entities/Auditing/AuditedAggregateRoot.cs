using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ddon.Domain.Entities.Auditing
{
    public class AuditedAggregateRoot<TKey> : AggregateRoot<TKey>, IAuditedObject, ISoftDelete
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 1001), JsonIgnore] public DateTime CreationTime { get; set; }

        [Column(Order = 1004), JsonIgnore] public DateTime? LastModificationTime { get; set; }

        [Column(Order = 9999), JsonIgnore] public bool IsDeleted { get; set; }
    }
}
