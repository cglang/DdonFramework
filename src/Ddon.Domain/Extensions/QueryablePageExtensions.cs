using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ddon.Domain.Extensions.ValueObject;
using Ddon.Domain.Specifications;

namespace Ddon.Domain.Extensions;

/// <summary>
/// Some useful extension methods for <see cref="IQueryable{TEntity}"/>.
/// </summary>
public static partial class QueryablePageExtensions
{
    /// <summary>
    /// 排序分页查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> QueryPageOrderBy<TEntity>(this IQueryable<TEntity> query, Page page)
    {
        return query.OrderBy(page.Sorting).Skip((page.Index - 1) * page.Size).Take(page.Size);
    }

    /// <summary>
    /// 排序分页查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> QueryPageOrderBy<TEntity>(this IQueryable<TEntity> query, Page page,
        Expression<Func<TEntity, object>> predicate)
    {
        return query.OrderBy(predicate).Skip((page.Index - 1) * page.Size).Take(page.Size);
    }

    /// <summary>
    /// 排序分页查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <param name="predicate"></param>
    /// <param name="orderType"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> QueryPageOrderBy<TEntity, TKey>(this IQueryable<TEntity> query, Page page,
        Expression<Func<TEntity, TKey>> predicate, OrderTypeEnum orderType = OrderTypeEnum.OrderBy)
    {
        return orderType switch
        {
            OrderTypeEnum.OrderBy => query.OrderBy(predicate).Skip((page.Index - 1) * page.Size).Take(page.Size),
            OrderTypeEnum.OrderByDescending => query.OrderByDescending(predicate).Skip((page.Index - 1) * page.Size)
                .Take(page.Size),
            _ => query.OrderBy(predicate).Skip((page.Index - 1) * page.Size).Take(page.Size),
        };
    }

    /// <summary>
    /// 排序分页查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <param name="orderExpressions"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> QueryPageOrderBy<TEntity, TKey>(this IQueryable<TEntity> query, Page page,
        IEnumerable<(Expression<Func<TEntity, TKey>> KeySelector, OrderTypeEnum OrderType)> orderExpressions)
    {
        var valueTuples = orderExpressions.ToList();
        if (!valueTuples.Any())
        {
            return query.OrderBy(page.Sorting).Skip((page.Index - 1) * page.Size).Take(page.Size);
        }

        IOrderedQueryable<TEntity>? orderedQuery = null;

        foreach (var (keySelector, orderType) in valueTuples)
        {
            orderedQuery = orderType switch
            {
                OrderTypeEnum.OrderBy => query.OrderBy(keySelector),
                OrderTypeEnum.OrderByDescending => query.OrderByDescending(keySelector),
                OrderTypeEnum.ThenBy => orderedQuery!.ThenBy(keySelector),
                OrderTypeEnum.ThenByDescending => orderedQuery!.ThenByDescending(keySelector),
                _ => orderedQuery
            };

            if (orderedQuery != null)
            {
                query = orderedQuery;
            }
        }

        return query.OrderBy(page.Sorting).Skip((page.Index - 1) * page.Size).Take(page.Size);
    }

    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> queryable, string propertyName)
    {
        return QueryableHelper<TEntity>.OrderBy(queryable, propertyName);
    }

    public static IQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> queryable,
        string propertyName)
    {
        return QueryableHelper<TEntity>.OrderByDescending(queryable, propertyName);
    }
}
