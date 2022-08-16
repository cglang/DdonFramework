namespace Ddon.Domain.Entities.Auditing
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
