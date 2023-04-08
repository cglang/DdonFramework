using System.Collections.Generic;

namespace Ddon.Repository.Dapper.SqlGenerator.QueryExpressions;

internal class QueryBinaryExpression : QueryExpression
{
    public QueryBinaryExpression(List<QueryExpression> nodes)
    {
        Nodes = nodes;
        NodeType = QueryExpressionType.Binary;
    }

    public List<QueryExpression> Nodes { get; }

    public override string ToString()
    {
        return $"[{base.ToString()} ({string.Join(",", Nodes)})]";
    }
}
