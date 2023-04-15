using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Ddon.Domain.Entities;

namespace Ddon.Domain.BaseObject
{
    public class AuditEntity<TKey> : Entity<TKey>, IAuditEntity, ISoftDelete
        where TKey : IEquatable<TKey>
    {
        [Column(Order = 101), JsonIgnore] public DateTime CreationTime { get; set; }

        [Column(Order = 111), JsonIgnore] public DateTime? LastModificationTime { get; set; }

        [Column(Order = 191), JsonIgnore] public bool IsDeleted { get; set; }
    }
}
