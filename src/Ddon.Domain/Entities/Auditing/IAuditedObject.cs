using System;

namespace Ddon.Domain.Entities.Auditing
{
    public interface IAuditedObject : ICreationAuditedObject
    {
        /// <summary>
        /// 最后一次修改时间
        /// </summary>
        DateTime? LastModificationTime { get; set; }
    }

    public interface IAuditedObject<TKey, TAuditKey> : IAuditedObject, ICreationAuditedObject<TKey, TAuditKey>
    {
        /// <summary>
        /// 最后一个修改该实体的用户ID
        /// </summary>
        TAuditKey? LastModifierId { get; set; }
    }

    public interface IAuditedObject<TKey, TAuditKey, TUser> : IAuditedObject<TKey, TAuditKey>, ICreationAuditedObject<TKey, TAuditKey, TUser>
    {
        /// <summary>
        /// 最后一个修改该实体的用户
        /// </summary>
        TUser? Modifier { get; set; }
    }
}