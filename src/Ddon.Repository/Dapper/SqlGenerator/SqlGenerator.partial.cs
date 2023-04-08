using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ddon.Repository.Dapper.SqlGenerator.QueryExpressions;

namespace Ddon.Repository.Dapper.SqlGenerator;

public partial class SqlGenerator<TEntity> where TEntity : class
{
    private QueryExpression GetQueryProperties(Expression expr, ExpressionType linkingType)
    {
        var isNotUnary = false;
        if (expr is UnaryExpression { NodeType: ExpressionType.Not, Operand: MethodCallExpression } unaryExpression)
        {
            expr = unaryExpression.Operand;
            isNotUnary = true;
        }

        switch (expr)
        {
            case MethodCallExpression methodCallExpression:
            {
                var methodName = methodCallExpression.Method.Name;
                var exprObj = methodCallExpression.Object;
                MethodLabel:
                switch (methodName)
                {
                    case nameof(IList.Contains):
                    {
                        if (exprObj is { NodeType: ExpressionType.MemberAccess } && exprObj.Type == typeof(string))
                        {
                            methodName = "StringContains";
                            goto MethodLabel;
                        }

                        var propertyName = ExpressionHelper.GetPropertyNamePath(methodCallExpression, out var isNested);

                        var propertyValue = ExpressionHelper.GetValuesFromCollection(methodCallExpression);
                        var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                        var link = ExpressionHelper.GetSqlOperator(linkingType);
                        return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                    }
                    case "StringContains":
                    case "CompareString":
                    case nameof(object.Equals):
                    case nameof(string.StartsWith):
                    case nameof(string.EndsWith):
                    {
                        if (exprObj is not { NodeType: ExpressionType.MemberAccess })
                        {
                            goto default;
                        }

                        var propertyName = ExpressionHelper.GetPropertyNamePath(exprObj, out bool isNested);

                        var propertyValue = ExpressionHelper.GetValuesFromStringMethod(methodCallExpression);
                        var likeValue = ExpressionHelper.GetSqlLikeValue(methodName, propertyValue);
                        var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                        var link = ExpressionHelper.GetSqlOperator(linkingType);
                        return new QueryParameterExpression(link, propertyName, likeValue, opr, isNested);
                    }
                    default:
                        throw new NotSupportedException($"'{methodName}' method is not supported");
                }
            }
            case BinaryExpression binaryExpression when binaryExpression.NodeType != ExpressionType.AndAlso &&
                                                        binaryExpression.NodeType != ExpressionType.OrElse:
            {
                var propertyName = ExpressionHelper.GetPropertyNamePath(binaryExpression, out var isNested);
                bool checkNullable = isNested && propertyName.EndsWith("HasValue");

                if (checkNullable)
                {
                    var prop = SqlProperties.FirstOrDefault(x =>
                        x.IsNullable && x.PropertyName + "HasValue" == propertyName);
                    if (prop != null)
                    {
                        isNested = false;
                        propertyName = prop.PropertyName;
                    }
                }

                var propertyValue = ExpressionHelper.GetValue(binaryExpression.Right);
                var nodeType = checkNullable
                    ? (propertyValue is false ? ExpressionType.Equal : ExpressionType.NotEqual)
                    : binaryExpression.NodeType;
                if (checkNullable)
                {
                    propertyValue = null;
                }

                var opr = ExpressionHelper.GetSqlOperator(nodeType);
                var link = ExpressionHelper.GetSqlOperator(linkingType);

                return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
            }
            case BinaryExpression binaryExpression:
            {
                var leftExpr = GetQueryProperties(binaryExpression.Left, ExpressionType.Default);
                var rightExpr = GetQueryProperties(binaryExpression.Right, binaryExpression.NodeType);

                switch (leftExpr)
                {
                    case QueryParameterExpression lQpExpr:
                        if (!string.IsNullOrEmpty(lQpExpr.LinkingOperator) &&
                            !string.IsNullOrEmpty(rightExpr.LinkingOperator)) // AND a AND B
                        {
                            switch (rightExpr)
                            {
                                case QueryBinaryExpression rQbExpr:
                                    if (lQpExpr.LinkingOperator ==
                                        rQbExpr.Nodes.Last().LinkingOperator) // AND a AND (c AND d)
                                    {
                                        var nodes = new QueryBinaryExpression(new List<QueryExpression> { leftExpr })
                                        {
                                            LinkingOperator = leftExpr.LinkingOperator,
                                        };

                                        rQbExpr.Nodes[0].LinkingOperator = rQbExpr.LinkingOperator;
                                        nodes.Nodes.AddRange(rQbExpr.Nodes);

                                        leftExpr = nodes;
                                        rightExpr = null;
                                        // AND a AND (c AND d) => (AND a AND c AND d)
                                    }

                                    break;
                            }
                        }

                        break;
                    case QueryBinaryExpression lQbExpr:
                        switch (rightExpr)
                        {
                            case QueryParameterExpression rQpExpr:
                                if (rQpExpr.LinkingOperator == lQbExpr.Nodes.Last().LinkingOperator) //(a AND b) AND c
                                {
                                    lQbExpr.Nodes.Add(rQpExpr);
                                    rightExpr = null;
                                    //(a AND b) AND c => (a AND b AND c)
                                }

                                break;

                            case QueryBinaryExpression rQbExpr:
                                if (lQbExpr.Nodes.Last().LinkingOperator ==
                                    rQbExpr.LinkingOperator) // (a AND b) AND (c AND d)
                                {
                                    if (rQbExpr.LinkingOperator ==
                                        rQbExpr.Nodes.Last().LinkingOperator) // AND (c AND d)
                                    {
                                        rQbExpr.Nodes[0].LinkingOperator = rQbExpr.LinkingOperator;
                                        lQbExpr.Nodes.AddRange(rQbExpr.Nodes);
                                        // (a AND b) AND (c AND d) =>  (a AND b AND c AND d)
                                    }
                                    else
                                    {
                                        lQbExpr.Nodes.Add(rQbExpr);
                                        // (a AND b) AND (c OR d) =>  (a AND b AND (c OR d))
                                    }

                                    rightExpr = null;
                                }

                                break;
                        }

                        break;
                }

                var nLinkingOperator = ExpressionHelper.GetSqlOperator(linkingType);
                if (rightExpr == null)
                {
                    leftExpr.LinkingOperator = nLinkingOperator;
                    return leftExpr;
                }

                return new QueryBinaryExpression(new List<QueryExpression> { leftExpr, rightExpr })
                {
                    NodeType = QueryExpressionType.Binary, LinkingOperator = nLinkingOperator,
                };
            }
            default:
                return GetQueryProperties(ExpressionHelper.GetBinaryExpression(expr), linkingType);
        }
    }
    
    
    public SqlQueryWhere AppendWherePredicateQuery(Expression<Func<TEntity, bool>>? predicate)
    {
        var sqlQueryWhere = new SqlQueryWhere();
        
        IDictionary<string, object?> dictionaryParams;
        if (sqlQueryWhere.Param is Dictionary<string, object?> param)
        {
            dictionaryParams = param;
        }
        else
        {
            dictionaryParams = new Dictionary<string, object?>();
        }

        if (predicate != null)
        {
            var queryProperties = GetQueryProperties(predicate.Body);

            sqlQueryWhere.SqlBuilder.Append("WHERE ");

            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object?>>();
            BuildQuerySql(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);
            sqlQueryWhere.SqlBuilder.Append(sqlBuilder);
            foreach (var condition in conditions)
            {
                dictionaryParams.Add(condition);
            }
        }

        sqlQueryWhere.SetParam(dictionaryParams);

        return sqlQueryWhere;
    }

    private void BuildQuerySql(IEnumerable<QueryExpression> queryProperties,
        ref StringBuilder sqlBuilder, ref List<KeyValuePair<string, object?>> conditions, ref int qLevel)
    {
        foreach (var expr in queryProperties)
        {
            if (!string.IsNullOrEmpty(expr.LinkingOperator))
            {
                if (sqlBuilder.Length > 0)
                    sqlBuilder.Append(" ");

                sqlBuilder.Append(expr.LinkingOperator).Append(" ");
            }

            switch (expr)
            {
                case QueryParameterExpression qpExpr:
                    var tableName = TableName;
                    var columnName = qpExpr.NestedProperty
                        ? string.Empty
                        : SqlProperties.First(x => x.PropertyName == qpExpr.PropertyName).ColumnName;

                    if (qpExpr.PropertyValue == null)
                    {
                        var inon = qpExpr.QueryOperator == "=" ? "IS" : "IS NOT";
                        sqlBuilder.AppendFormat($"{tableName}.{columnName} {inon} NULL");
                    }
                    else
                    {
                        var vKey = $"{qpExpr.PropertyName}_p{qLevel}"; //Handle multiple uses of a field
                        sqlBuilder.AppendFormat(
                            $"{tableName}.{columnName} {qpExpr.QueryOperator} {ParameterSymbol}{vKey}");
                        conditions.Add(new KeyValuePair<string, object?>(vKey, qpExpr.PropertyValue));
                    }

                    qLevel++;
                    break;

                case QueryBinaryExpression qbExpr:
                    var nSqlBuilder = new StringBuilder();
                    var nConditions = new List<KeyValuePair<string, object?>>();
                    BuildQuerySql(qbExpr.Nodes, ref nSqlBuilder, ref nConditions, ref qLevel);

                    if (qbExpr.Nodes.Count == 1) //Handle `grouping brackets`
                        sqlBuilder.Append(nSqlBuilder);
                    else
                        sqlBuilder.AppendFormat($"({nSqlBuilder})");

                    conditions.AddRange(nConditions);
                    break;
            }
        }
    }

    private IEnumerable<QueryExpression> GetQueryProperties(Expression expr)
    {
        var queryNode = GetQueryProperties(expr, ExpressionType.Default);
        return queryNode switch
        {
            QueryParameterExpression => new List<QueryExpression> { queryNode },
            QueryBinaryExpression qbExpr => qbExpr.Nodes,
            _ => throw new NotSupportedException(queryNode.ToString())
        };
    }
}
