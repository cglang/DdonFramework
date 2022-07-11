namespace Ddon.Core.Services.Guids
{
    /// <summary>
    /// 顺序 GUID 类型
    /// </summary>
    public enum SequentialGuidType
    {
        /// <summary>
        /// 适用于 MySQL 和 PostgreSql
        /// </summary>
        SequentialAsString,

        /// <summary>
        /// 适用于 Oracle
        /// </summary>
        SequentialAsBinary,

        /// <summary>
        /// 适用于 SqlServer
        /// </summary>
        SequentialAtEnd
    }
}