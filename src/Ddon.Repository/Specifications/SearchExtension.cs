using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ddon.Repositiry.Specifications
{
    public static class SearchExtension
    {
        public static IQueryable<T> Search<T>(this IQueryable<T> source, IEnumerable<(Expression<Func<T, string>> selector, string searchTerm)> criterias)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            Expression? expr = null;

            foreach (var (selector, searchTerm) in criterias)
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    continue;
                }

                var functions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions))!);
                var like = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like), new Type[] { functions.Type, typeof(string), typeof(string) });

                var propertySelector = ParameterReplacerVisitor.Replace(selector, selector.Parameters[0], parameter);

                var likeExpression = Expression.Call(null, like!, functions, (propertySelector as LambdaExpression)?.Body!, Expression.Constant(searchTerm));

                expr = expr == null ? likeExpression : Expression.OrElse(expr, likeExpression);
            }

            return expr != null ? source.Where(Expression.Lambda<Func<T, bool>>(expr, parameter)) : source;
        }
    }
}
