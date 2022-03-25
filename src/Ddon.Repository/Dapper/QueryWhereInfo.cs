namespace Ddon.Repositiry.Dapper
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryWhereInfo
    {
        public QueryWhereInfo(string whereSql, AndOr andOr = AndOr.And)
        {
            WhereSql = whereSql;
            AndOr = andOr;
        }

        /// <summary>
        ///  完整的 where 条件语句
        ///  例：o.OrderNo = @OrderNo
        /// </summary>
        public string WhereSql { get; set; }

        /// <summary>
        /// WHERE 条件之间的"与"和"或"关系
        /// </summary>
        public AndOr AndOr { get; set; }
    }
}
