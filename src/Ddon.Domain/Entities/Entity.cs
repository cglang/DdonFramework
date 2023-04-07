using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Domain.Entities
{
    public class Entity<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
    {
        [Key, NotNull, Column(Order = 0)]
        public TKey Id { get; set; } = default!;
    }
}
