using System;

namespace Ddon.Domain.Entities.Auditing
{
    public interface IAuditedObject
    {
        /// <summary>
        /// ����ʱ��
        /// </summary>
        DateTime CreationTime { get; set; }

        /// <summary>
        /// ���һ���޸�ʱ��
        /// </summary>
        DateTime? LastModificationTime { get; set; }
    }
}