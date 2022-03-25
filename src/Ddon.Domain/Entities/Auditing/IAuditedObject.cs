using System;

namespace Ddon.Domain.Entities.Auditing
{
    public interface IAuditedObject : ICreationAuditedObject
    {
        /// <summary>
        /// ���һ���޸�ʱ��
        /// </summary>
        DateTime? LastModificationTime { get; set; }
    }

    public interface IAuditedObject<TKey, TAuditKey> : IAuditedObject, ICreationAuditedObject<TKey, TAuditKey>
    {
        /// <summary>
        /// ���һ���޸ĸ�ʵ����û�ID
        /// </summary>
        TAuditKey? LastModifierId { get; set; }
    }

    public interface IAuditedObject<TKey, TAuditKey, TUser> : IAuditedObject<TKey, TAuditKey>, ICreationAuditedObject<TKey, TAuditKey, TUser>
    {
        /// <summary>
        /// ���һ���޸ĸ�ʵ����û�
        /// </summary>
        TUser? Modifier { get; set; }
    }
}