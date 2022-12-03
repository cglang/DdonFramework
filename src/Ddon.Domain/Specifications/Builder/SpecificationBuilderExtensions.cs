using Ddon.Domain.Dtos;
using Ddon.Domain.Entities;
using Ddon.Domain.Specifications.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ddon.Domain.Specifications.Builder
{
    public static class SpecificationBuilderExtensions
    {
        public static ISpecificationBuilder<TEntity> Where<TEntity>(this ISpecificationBuilder<TEntity> specificationBuilder, Expression<Func<TEntity, bool>> criteria)
        {
            ((List<Expression<Func<TEntity, bool>>>)specificationBuilder.Specification.WhereExpressions).Add(criteria);

            return specificationBuilder;
        }

        public static IOrderedSpecificationBuilder<TEntity> OrderBy<TEntity>(this ISpecificationBuilder<TEntity> specificationBuilder, Expression<Func<TEntity, object>> orderExpression)
        {
            ((List<(Expression<Func<TEntity, object>> OrderExpression, OrderTypeEnum OrderType)>)specificationBuilder.Specification.OrderExpressions).Add((orderExpression, OrderTypeEnum.OrderBy));

            var orderedSpecificationBuilder = new OrderedSpecificationBuilder<TEntity>(specificationBuilder.Specification);

            return orderedSpecificationBuilder;
        }

        public static IOrderedSpecificationBuilder<TEntity> OrderByDescending<TEntity>(this ISpecificationBuilder<TEntity> specificationBuilder, Expression<Func<TEntity, object>> orderExpression)
        {
            ((List<(Expression<Func<TEntity, object>> OrderExpression, OrderTypeEnum OrderType)>)specificationBuilder.Specification.OrderExpressions).Add((orderExpression, OrderTypeEnum.OrderByDescending));

            var orderedSpecificationBuilder = new OrderedSpecificationBuilder<TEntity>(specificationBuilder.Specification);

            return orderedSpecificationBuilder;
        }

        public static IIncludableSpecificationBuilder<TEntity, TProperty> Include<TEntity, TProperty>(this ISpecificationBuilder<TEntity> specificationBuilder, Expression<Func<TEntity, TProperty>> includeExpression)
        {
            var aggregator = new IncludeAggregator((includeExpression.Body as MemberExpression)?.Member?.Name);
            var includeBuilder = new IncludableSpecificationBuilder<TEntity, TProperty>(specificationBuilder.Specification, aggregator);

            ((List<IIncludeAggregator>)specificationBuilder.Specification.IncludeAggregators).Add(aggregator);

            return includeBuilder;
        }

        public static ISpecificationBuilder<TEntity> Include<TEntity>(this ISpecificationBuilder<TEntity> specificationBuilder, string includeString)
        {
            ((List<string>)specificationBuilder.Specification.IncludeStrings).Add(includeString);
            return specificationBuilder;
        }


        public static ISpecificationBuilder<TEntity> Search<TEntity>(this ISpecificationBuilder<TEntity> specificationBuilder, Expression<Func<TEntity, string>> selector, string searchTerm, int searchGroup = 1)
        {
            ((List<(Expression<Func<TEntity, string>> Selector, string SearchTerm, int SearchGroup)>)specificationBuilder.Specification.SearchCriterias).Add((selector, searchTerm, searchGroup));

            return specificationBuilder;
        }

        public static ISpecificationBuilder<TEntity> Page<TEntity>(this ISpecificationBuilder<TEntity> specificationBuilder, Page page)
        {
            if (specificationBuilder.Specification.Page != null) throw new DuplicateTakeException();

            specificationBuilder.Specification.Page = page;
            specificationBuilder.Specification.IsPagingEnabled = true;
            return specificationBuilder;
        }

        /// <summary>
        /// Must be called after specifying criteria
        /// </summary>
        /// <param name="specificationName"></param>
        /// <param name="args">Any arguments used in defining the specification</param>
        public static ISpecificationBuilder<TEntity> EnableCache<TEntity>(
            this ISpecificationBuilder<TEntity> specificationBuilder,
            string specificationName,
            params object[] args
            )
        {
            System.Diagnostics.Debug.Assert(string.IsNullOrWhiteSpace(specificationName));
            System.Diagnostics.Debug.Assert(specificationBuilder.Specification.WhereExpressions == null);

            specificationBuilder.Specification.CacheKey = $"{specificationName}-{string.Join("-", args)}";

            specificationBuilder.Specification.CacheEnabled = true;

            return specificationBuilder;
        }

        public static ISpecificationBuilder<TEntity, TKey, TResult> Select<TEntity, TKey, TResult>(
            this ISpecificationBuilder<TEntity, TKey, TResult> specificationBuilder,
            Expression<Func<TEntity, TResult>> selector)
            where TEntity : IEntity<TKey>
            where TKey : IEquatable<TKey>
        {
            specificationBuilder.Specification.Selector = selector;

            return specificationBuilder;
        }
    }
}