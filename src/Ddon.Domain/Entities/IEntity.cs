namespace Ddon.Domain.Entities
{
    public interface IEntity
    {
        object[] GetKeys();
    }

    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; set; }
    }
}
