using System;

namespace Ddon.Domain.BaseObject
{
    public interface IAuditEntity
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreationTime { get; set; }

        /// <summary>
        /// 最后一次修改时间
        /// </summary>
        DateTime? LastModificationTime { get; set; }
    }
}
