using Ddon.Domain.Dtos;
using Ddon.Domain.Entities;
using Ddon.Domain.Specifications.Builder;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ddon.Domain.Specifications
{
    public abstract class Specification<T> : ISpecification<T>
    {
        protected virtual ISpecificationBuilder<T> Query { get; }

        public IEnumerable<Expression<Func<T, bool>>> WhereExpressions { get; } = new List<Expression<Func<T, bool>>>();

        public IEnumerable<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)> OrderExpressions { get; } = new List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>();

        public IEnumerable<IIncludeAggregator> IncludeAggregators { get; } = new List<IIncludeAggregator>();

        public IEnumerable<string> IncludeStrings { get; } = new List<string>();

        public Expression<Func<T, object>>[] IncludePropertySelectors => throw new NotImplementedException();

        public IEnumerable<(Expression<Func<T, string>> selector, string searchTerm, int searchGroup)> SearchCriterias { get; } = new List<(Expression<Func<T, string>> Selector, string SearchTerm, int SearchGroup)>();

        public Page? Page { get; internal set; } = null;

        public bool IsPagingEnabled { get; internal set; } = false;

        public string CacheKey { get; internal set; }

        protected Specification(string cacheKey)
        {
            Query = new SpecificationBuilder<T>(this);
            CacheKey = cacheKey;
        }

        public bool CacheEnabled { get; internal set; }

    }

    public abstract class Specification<T, TKey, TResult> : Specification<T>, ISpecification<T, TKey, TResult>
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        protected new virtual ISpecificationBuilder<T, TKey, TResult> Query { get; }

        public Expression<Func<T, TResult>> Selector { get; internal set; }

        protected Specification(string cacheKey, Expression<Func<T, TResult>> selector) : base(cacheKey)
        {
            Selector = selector;
            Query = new SpecificationBuilder<T, TKey, TResult>(this);
        }
    }
}