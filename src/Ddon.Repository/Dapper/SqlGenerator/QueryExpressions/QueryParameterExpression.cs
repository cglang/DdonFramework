namespace Ddon.Repository.Dapper.SqlGenerator.QueryExpressions;

/// <inheritdoc />
/// <summary>
/// Class that models the data structure in coverting the expression tree into SQL and Params.
/// `Parameter` Query Expression
/// </summary>
internal class QueryParameterExpression : QueryExpression
{
    private QueryParameterExpression()
    {
        NodeType = QueryExpressionType.Parameter;
    }

    internal QueryParameterExpression(string linkingOperator,
        string propertyName, object? propertyValue,
        string queryOperator, bool nestedProperty) : this()
    {
        LinkingOperator = linkingOperator;
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        QueryOperator = queryOperator;
        NestedProperty = nestedProperty;
    }

    public string? PropertyName { get; set; }
    public object? PropertyValue { get; set; }
    public string? QueryOperator { get; set; }
    public bool NestedProperty { get; set; }

    public override string ToString()
    {
        return
            $"[{base.ToString()}, PropertyName:{PropertyName}, PropertyValue:{PropertyValue}, QueryOperator:{QueryOperator}, NestedProperty:{NestedProperty}]";
    }
}
