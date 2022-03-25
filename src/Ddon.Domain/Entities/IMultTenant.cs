namespace Ddon.Domain.Entities
{
    public interface IMultTenant { }

    public interface IMultTenant<TKey> : IMultTenant
    {
        TKey TenantId { get; set; }
    }
}
