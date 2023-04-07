using System.Diagnostics.CodeAnalysis;

namespace Ddon.Domain.Entities
{
    public interface IEntity { }

    public interface IEntity<TKey> : IEntity
    {
        [NotNull]
        TKey Id { get; set; }

        public virtual object[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
