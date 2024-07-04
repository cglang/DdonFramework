using System;
using System.Linq;
using Ddon.Domain.Entities;
using Ddon.Domain.Specifications;
using Ddon.Domain.Specifications.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Repository.EntityFrameworkCore.Specifications
{
    public class SpecificationEvaluator<TEntity> : ISpecificationEvaluator<TEntity> where TEntity : class, IEntity
    {
        public virtual IQueryable<TEntity> BuildQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
        {
            if (specification is null) throw new ArgumentNullException(nameof(specification));

            var query = inputQuery;

            foreach (var includeString in specification.IncludeStrings)
            {
                query = query.Include(includeString);
            }

            foreach (var includeAggregator in specification.IncludeAggregators)
            {
                var includeString = includeAggregator.IncludeString;
                if (!string.IsNullOrEmpty(includeString))
                {
                    query = query.Include(includeString);
                }
            }

            foreach (var propertySelector in specification.IncludePropertySelectors)
            {
                query = query.Include(propertySelector);
            }

            foreach (var criteria in specification.WhereExpressions)
            {
                query = query.Where(criteria);
            }

            foreach (var searchCriteria in specification.SearchCriterias.GroupBy(x => x.searchGroup))
            {
                var criterias = searchCriteria.Select(x => (x.selector, x.searchTerm));
                query = query.Search(criterias);
            }

            if (specification.Page != null)
            {
                query = query.QueryPageOrderBy(specification.Page, specification.OrderExpressions);
            }

            return query;
        }
    }

    public class SpecificationEvaluator<TEntity, TKey> : SpecificationEvaluator<TEntity>, ISpecificationEvaluator<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        public virtual IQueryable<TResult> BuildQuery<TResult>(IQueryable<TEntity> inputQuery, ISpecification<TEntity, TKey, TResult> specification)
        {
            if (specification is null) throw new ArgumentNullException(nameof(specification));
            if (specification.Selector is null) throw new SelectorNotFoundException();

            var query = BuildQuery(inputQuery, (ISpecification<TEntity>)specification);

            var selectQuery = query.Select(specification.Selector);

            return selectQuery;
        }
    }
}
