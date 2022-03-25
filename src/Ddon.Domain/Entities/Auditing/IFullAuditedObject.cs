namespace Ddon.Domain.Entities.Auditing
{
    public interface IFullAuditedObject : IAuditedObject, ISoftDelete
    {

    }


    public interface IFullAuditedObject<TKey, TAuditKey> :
        IFullAuditedObject, IAuditedObject<TKey, TAuditKey>
    {
        TKey? DeleterId { get; set; }
    }


    public interface IFullAuditedObject<TKey, TAuditKey, TUser> :
        IFullAuditedObject<TKey, TAuditKey>, IAuditedObject<TKey, TAuditKey, TUser>
    {
        TUser? Deleter { get; set; }
    }
}