using Ddon.Domain;
using Ddon.Domain.Entities;
using Ddon.Identity.Specifications.Builder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ddon.Identity.Specifications
{
    public interface ISpecification<TEntity>
    {
        IEnumerable<Expression<Func<TEntity, bool>>> WhereExpressions { get; }

        IEnumerable<(Expression<Func<TEntity, object>> KeySelector, OrderTypeEnum OrderType)> OrderExpressions { get; }

        IEnumerable<IIncludeAggregator> IncludeAggregators { get; }

        IEnumerable<string> IncludeStrings { get; }

        Expression<Func<TEntity, object>>[] IncludePropertySelectors { get; }

        IEnumerable<(Expression<Func<TEntity, string>> selector, string searchTerm, int searchGroup)> SearchCriterias { get; }

        Page? Page { get; }

        bool CacheEnabled { get; }

        string CacheKey { get; }
    }

    public interface ISpecification<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        IEnumerable<Expression<Func<TEntity, bool>>> WhereExpressions { get; }

        IEnumerable<(Expression<Func<TEntity, object>> KeySelector, OrderTypeEnum OrderType)> OrderExpressions { get; }

        IEnumerable<IIncludeAggregator> IncludeAggregators { get; }

        IEnumerable<string> IncludeStrings { get; }

        Expression<Func<TEntity, object>>[] IncludePropertySelectors { get; }

        IEnumerable<(Expression<Func<TEntity, string>> selector, string searchTerm, int searchGroup)> SearchCriterias { get; }

        Page? Page { get; }

        bool CacheEnabled { get; }

        string CacheKey { get; }
    }

    public interface ISpecification<TEntity, TKey, TResult> : ISpecification<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        Expression<Func<TEntity, TResult>> Selector { get; }
    }
}