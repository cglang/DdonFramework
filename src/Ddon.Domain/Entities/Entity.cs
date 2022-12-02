using System;
using System.ComponentModel.DataAnnotations;

namespace Ddon.Domain.Entities
{
    public abstract class Entity : IEntity
    {
        public abstract object[] GetKeys();
    }

    public class Entity<TKey> : Entity, IEntity<TKey> where TKey : IEquatable<TKey>
    {
        [Key]
        public TKey Id { get; set; } = default!;

        public override object[] GetKeys()
        {
            return new object[] { Id! };
        }
    }
}
