using Ddon.Domain.Dtos;
using Ddon.Domain.Specifications;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    /// <summary>
    /// Some useful extension methods for <see cref="IQueryable{TEntity}"/>.
    /// </summary>
    public static class QueryablePageExtensions
    {
        /// <summary>
        /// 排序分页查询
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<PageResult<TEntity>> QueryPageOrderByAsync<TEntity>(
            this IQueryable<TEntity> query,
            Page page, 
            CancellationToken cancellationToken = default)
        {
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderBy(page.Sorting)
                .Skip((page.Index - 1) * page.Size).Take(page.Size).ToListAsync(cancellationToken);
            return new PageResult<TEntity> { Total = total, Items = data };
        }

        /// <summary>
        /// 排序分页查询
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<PageResult<TEntity>> QueryPageOrderByAsync<TEntity>(
            this IQueryable<TEntity> query,
            Page page,
            Expression<Func<TEntity, object>> predicate, 
            CancellationToken cancellationToken = default)
        {
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderBy(predicate)
                .Skip((page.Index - 1) * page.Size).Take(page.Size).ToListAsync(cancellationToken);
            return new PageResult<TEntity> { Total = total, Items = data };
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<PageResult<TEntity>> QueryPageOrderByAsync<TEntity, TKey>(
            this IQueryable<TEntity> query, Page page,
            Expression<Func<TEntity, TKey>> predicate, 
            OrderTypeEnum orderType = OrderTypeEnum.OrderBy, 
            CancellationToken cancellationToken = default)
        {
            long total;
            IEnumerable<TEntity> data;
            switch (orderType)
            {
                case OrderTypeEnum.OrderByDescending:
                case OrderTypeEnum.ThenByDescending:
                    total = await query.CountAsync(cancellationToken);
                    data = await query.OrderByDescending(predicate).Skip((page.Index - 1) * page.Size).Take(page.Size)
                        .ToListAsync(cancellationToken);
                    return new PageResult<TEntity> { Total = total, Items = data };
                case OrderTypeEnum.OrderBy:
                case OrderTypeEnum.ThenBy:
                default:
                    total = await query.CountAsync(cancellationToken);
                    data = await query.OrderBy(predicate).Skip((page.Index - 1) * page.Size).Take(page.Size)
                        .ToListAsync(cancellationToken);
                    return new PageResult<TEntity> { Total = total, Items = data };
            }
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
        public static IQueryable<TEntity> QueryPageOrderBy<TEntity, TKey>(
            this IQueryable<TEntity> query, 
            Page page,
            IEnumerable<(Expression<Func<TEntity, TKey>> KeySelector, OrderTypeEnum OrderType)> orderExpressions)
        {
            var valueTuples = orderExpressions.ToList();
            if (valueTuples.Any())
            {
                IOrderedQueryable<TEntity>? orderedQuery = null;

                foreach (var (keySelector, orderType) in valueTuples)
                {
                    switch (orderType)
                    {
                        case OrderTypeEnum.OrderBy:
                            orderedQuery = query.OrderBy(keySelector);
                            break;
                        case OrderTypeEnum.OrderByDescending:
                            orderedQuery = query.OrderByDescending(keySelector);
                            break;
                        case OrderTypeEnum.ThenBy:
                            orderedQuery = orderedQuery!.ThenBy(keySelector);
                            break;
                        case OrderTypeEnum.ThenByDescending:
                            orderedQuery = orderedQuery!.ThenByDescending(keySelector);
                            break;
                    }

                    if (orderedQuery != null)
                    {
                        query = orderedQuery;
                    }
                }
            }

            return query.OrderBy(page.Sorting).Skip((page.Index - 1) * page.Size).Take(page.Size);
        }


        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> queryable, string propertyName)
        {
            return QueryableHelper.OrderBy(queryable, propertyName);
        }

        public static IQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> queryable,
            string propertyName)
        {
            return QueryableHelper.OrderByDescending(queryable, propertyName);
        }
    }

    public static class QueryableHelper
    {
        private static readonly ConcurrentDictionary<string, LambdaExpression> Cached = new();

        public static IQueryable<TEntity> OrderBy<TEntity>(IQueryable<TEntity> queryable, string propertyName)
        {
            dynamic keySelector = GetLambdaExpression<TEntity>(propertyName);
            return Queryable.OrderBy(queryable, keySelector);
        }

        public static IQueryable<TEntity> OrderByDescending<TEntity>(IQueryable<TEntity> queryable, string propertyName)
        {
            dynamic keySelector = GetLambdaExpression<TEntity>(propertyName);
            return Queryable.OrderByDescending(queryable, keySelector);
        }

        private static LambdaExpression GetLambdaExpression<TEntity>(string propertyName)
        {
            var key = $"{typeof(TEntity).FullName}.{propertyName}";
            if (Cached.ContainsKey(key))
            {
                return Cached[key];
            }

            var param = Expression.Parameter(typeof(TEntity));
            var body = Expression.Property(param, propertyName);
            var keySelector = Expression.Lambda(body, param);
            Cached[key] = keySelector;

            return keySelector;
        }
    }
}
