namespace Ddon.Domain.BaseObject
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
