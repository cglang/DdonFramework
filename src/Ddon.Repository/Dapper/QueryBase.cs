using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ddon.Repository.Dapper;

public abstract class QueryBase
{
    private bool _isResolveCondition;

    /// <summary>
    /// 是否只查询软删除数据
    /// </summary>
    private readonly bool _isNotQueryDelete;

    /// <summary>
    /// WHERE 查询条件 "AND模式"
    /// </summary>
    private readonly List<QueryWhereInfo> _wheres = new();

    /// <summary>
    /// 排序字段
    /// </summary>
    private readonly List<string> _sorts = new();

    protected QueryBase(bool isNotQueryDelete = false)
    {
        _isNotQueryDelete = isNotQueryDelete;
    }

    /// <summary>
    /// SELECT 部分的 SQL
    /// </summary>
    /// <returns></returns>
    protected abstract string SelectSql();

    /// <summary>
    /// FROM 部分的 SQL
    /// </summary>
    /// <returns></returns>
    protected abstract string FromSql();

    // /// <summary>
    // /// JOIN 部分的 SQL
    // /// </summary>
    // /// <returns></returns>
    // protected abstract string JoinSql();

    /// <summary>
    /// WHERE 条件在此方法内进行添加
    /// </summary>
    /// <returns></returns>
    protected abstract void WhereSql();

    /// <summary>
    /// 排序条件在此方法内进行添加
    /// </summary>
    /// <returns></returns>
    protected abstract void SortSql();


    protected virtual void AddWhere(string whereString, AndOr andOr = AndOr.And)
    {
        _wheres.Add(new QueryWhereInfo(whereString, andOr));
    }


    protected void AddSort(string sortString, SortType sortType = SortType.Asc)
    {
        var sort = $"{sortString} {sortType}";
        if (!_sorts.Contains(sort))
        {
            _sorts.Add(sort);
        }
    }

    /// <summary>
    /// 获取完整的查询 SQL
    /// </summary>
    /// <returns></returns>
    public virtual string BuildCompleteSql()
    {
        InitCondition();

        StringBuilder sqlBuilder = new();
        sqlBuilder.Append(BuildSelectSql()).Append(BuildFromSql()).Append(BuildWhereSql()).Append(BuildOrderBy());

        return sqlBuilder.ToString();
    }

    public StringBuilder BuildSelectSql()
    {
        InitCondition();

        StringBuilder sqlBuilder = new();
        sqlBuilder.Append(SelectSql()).Append(Environment.NewLine);
        return sqlBuilder;
    }

    public StringBuilder BuildFromSql()
    {
        InitCondition();

        StringBuilder sqlBuilder = new();
        var fromSql = FromSql();
        if (fromSql.IsNullOrWhiteSpace())
        {
            return sqlBuilder;
        }

        sqlBuilder.Append(FromSql()).Append(Environment.NewLine);

        // var joinSql = JoinSql();
        // if (!joinSql.IsNullOrWhiteSpace())
        // {
        //     sqlBuilder.Append(JoinSql()).Append(Environment.NewLine);
        // }

        return sqlBuilder;
    }

    public StringBuilder BuildWhereSql()
    {
        InitCondition();

        StringBuilder sqlBuilder = new();
        var whereSql = string.Join(string.Empty, _wheres.Select(x => $" {x.AndOr} {x.WhereSql}"));
        sqlBuilder.Append("WHERE 1 = 1").Append(whereSql).Append(Environment.NewLine);

        return sqlBuilder;
    }

    public StringBuilder BuildOrderBy()
    {
        InitCondition();

        StringBuilder sqlBuilder = new();

        if (_sorts.Any())
        {
            sqlBuilder.Append("ORDER BY ").Append(string.Join(",", _sorts));
        }

        return sqlBuilder;
    }

    private void InitCondition()
    {
        if (_isResolveCondition) return;
        lock (this)
        {
            if (_isResolveCondition)
            {
                return;
            }

            WhereSql();
            SortSql();

            if (_isNotQueryDelete)
            {
                AddWhere("IsDeleted = 0");
            }

            _isResolveCondition = true;
        }
    }
}

public abstract class QueryPageBase : QueryBase
{
    private readonly int _maxPageCount;

    /// <summary>
    /// 单页容量(最大20万)
    /// </summary>
    public int PageCount
    {
        get => _maxPageCount >= 200000 ? 200000 : _maxPageCount;
        init => _maxPageCount = value;
    }

    public int PageIndex { get; init; }

    /// <summary>
    /// 获取完整的查询 SQL
    /// </summary>
    /// <returns></returns>
    public override string BuildCompleteSql()
    {
        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append(base.BuildCompleteSql()).Append(Environment.NewLine);
        sqlBuilder.Append(BuildPageSql());
        return sqlBuilder.ToString();
    }

    public StringBuilder BuildPageSql()
    {
        // TODO 提供不同的SQL分页方式
        
        var sqlBuilder = new StringBuilder();
        
        var tempMaxResultCount = PageCount <= 0 ? 0 : PageCount;
        var tempSkipCount = (PageIndex <= 0 ? 0 : PageIndex - 1) * PageCount;
        sqlBuilder.Append($"OFFSET {tempSkipCount} ROWS FETCH NEXT {tempMaxResultCount} ROWS ONLY");
        
        return sqlBuilder;
    }
}

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

public enum AndOr
{
    And,
    Or
}

public enum SortType
{
    /// <summary>
    /// 升序
    /// </summary>
    Asc,

    /// <summary>
    /// 降序
    /// </summary>
    Desc
}

public class PageResult<T>
{
    public PageResult(IEnumerable<T> items, long totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }

    /// <summary>
    ///  集合数据
    /// </summary>
    public IEnumerable<T> Items { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public long TotalCount { get; set; }
}
