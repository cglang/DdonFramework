using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Ddon.Domain.Extensions;

public static partial class QueryablePageExtensions
{
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
            var key = $"{typeof(TEntity).FullName}.{propertyName}";
            if (Cached.TryGetValue(key, out var value))
            {
                return value;
            }

            var param = Expression.Parameter(typeof(TEntity));
            var body = Expression.Property(param, propertyName);
            var keySelector = Expression.Lambda(body, param);
            Cached[key] = keySelector;

            return keySelector;
        }
    }
}
