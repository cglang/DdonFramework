using Ddon.Domain;
using Ddon.Identity.Specifications;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

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
                OrderTypeEnum.OrderByDescending => query.OrderByDescending(predicate).Skip((page.Index - 1) * page.Size).Take(page.Size),
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
            return QueryableHelper<TEntity>.OrderBy(queryable, propertyName);
        }

        public static IQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> queryable,
            string propertyName)
        {
            return QueryableHelper<TEntity>.OrderByDescending(queryable, propertyName);
        }

        public class QueryableHelper
        {
            protected static readonly ConcurrentDictionary<string, LambdaExpression> Cached = new();
        }

        private class QueryableHelper<TEntity> : QueryableHelper
        {
            public static IQueryable<TEntity> OrderBy(IQueryable<TEntity> queryable, string propertyName)
            {
                dynamic keySelector = GetLambdaExpression(propertyName);
                return Queryable.OrderBy(queryable, keySelector);
            }

            public static IQueryable<TEntity> OrderByDescending(IQueryable<TEntity> queryable, string propertyName)
            {
                dynamic keySelector = GetLambdaExpression(propertyName);
                return Queryable.OrderByDescending(queryable, keySelector);
            }

            private static LambdaExpression GetLambdaExpression(string propertyName)
            {
                if (Cached.ContainsKey(propertyName))
                {
                    return Cached[propertyName];
                }
                var param = Expression.Parameter(typeof(TEntity));
                var body = Expression.Property(param, propertyName);
                var keySelector = Expression.Lambda(body, param);
                Cached[propertyName] = keySelector;

                return keySelector;
            }
        }
    }
}
