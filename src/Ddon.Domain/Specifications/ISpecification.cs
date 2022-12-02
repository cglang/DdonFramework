using Ddon.Domain.Dtos;
using Ddon.Domain.Entities;
using Ddon.Domain.Specifications.Builder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ddon.Domain.Specifications
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

    public interface ISpecification<TEntity, TKey> where TEntity : IEntity<TKey>
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

    public interface ISpecification<TEntity, TKey, TResult> : ISpecification<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        Expression<Func<TEntity, TResult>> Selector { get; }
    }
}