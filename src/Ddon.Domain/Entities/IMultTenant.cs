namespace Ddon.Domain.Entities
{
    public interface IMultTenant<TKey> : IEntity<TKey>
    {
        TKey? TenantId { get; set; }
    }
}
