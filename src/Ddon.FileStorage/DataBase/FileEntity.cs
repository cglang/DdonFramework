using Ddon.Domain.Entities.Auditing;

namespace Ddon.FileStorage.DataBase
{
    public class FileEntity : AuditedAggregateRoot<string>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 文件描述
        /// </summary>
        public string? Describe { get; set; }

        /// <summary>
        /// 相对路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 全路径
        /// </summary>
        public string FullPath { get; set; } = "";

        /// <summary>
        /// 拓展名
        /// </summary>
        public string? Extension { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
    }
}
