namespace Ddon.Core.Services.Guids
{
    /// <summary>
    /// Describes the type of a sequential GUID value.
    /// </summary>
    public enum SequentialGuidType
    {
        /// <summary>
        /// Used by MySql and PostgreSql.
        /// </summary>
        SequentialAsString,

        /// <summary>
        /// Used by Oracle.
        /// </summary>
        SequentialAsBinary,

        /// <summary>
        /// Used by SqlServer.
        /// </summary>
        SequentialAtEnd
    }
}