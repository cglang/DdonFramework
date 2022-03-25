using System;

namespace Ddon.Domain.Entities.Auditing
{
    public interface ICreationAuditedObject : IEntity
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreationTime { get; set; }
    }

    public interface ICreationAuditedObject<TKey, TAuditKey> : ICreationAuditedObject, IEntity<TKey>
    {
        /// <summary>
        /// 创建用户的Id
        /// </summary>
        TAuditKey? CreatorId { get; set; }
    }

    public interface ICreationAuditedObject<TKey, TAuditKey, TUser> : ICreationAuditedObject<TKey, TAuditKey>
    {
        /// <summary>
        /// 创建用户的Id
        /// </summary>
        TUser? Creator { get; set; }
    }
}