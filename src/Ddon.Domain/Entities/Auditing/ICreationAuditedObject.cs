using System;

namespace Ddon.Domain.Entities.Auditing
{
    public interface ICreationAuditedObject : IEntity
    {
        /// <summary>
        /// ����ʱ��
        /// </summary>
        DateTime CreationTime { get; set; }
    }

    public interface ICreationAuditedObject<TKey, TAuditKey> : ICreationAuditedObject, IEntity<TKey>
    {
        /// <summary>
        /// �����û���Id
        /// </summary>
        TAuditKey? CreatorId { get; set; }
    }

    public interface ICreationAuditedObject<TKey, TAuditKey, TUser> : ICreationAuditedObject<TKey, TAuditKey>
    {
        /// <summary>
        /// �����û���Id
        /// </summary>
        TUser? Creator { get; set; }
    }
}