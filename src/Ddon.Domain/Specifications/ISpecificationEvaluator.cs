using Ddon.Domain.Entities;
using System.Linq;

namespace Ddon.Identity.Specifications
{
    public interface ISpecificationEvaluator<TEntity> where TEntity : class, IEntity
    {
        IQueryable<TEntity> BuildQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification);
    }

    public interface ISpecificationEvaluator<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        IQueryable<TResult> BuildQuery<TResult>(IQueryable<TEntity> inputQuery, ISpecification<TEntity, TKey, TResult> specification);
    }
}