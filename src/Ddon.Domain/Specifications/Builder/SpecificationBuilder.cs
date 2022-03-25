using Ddon.Domain.Entities;
using System;

namespace Ddon.Identity.Specifications.Builder
{
    public class SpecificationBuilder<TEntity, TKey, TResult> : SpecificationBuilder<TEntity>, ISpecificationBuilder<TEntity, TKey, TResult>
        where TKey : IEquatable<TKey>
        where TEntity : TenantEntity<TKey>
    {
        public new Specification<TEntity, TKey, TResult> Specification { get; }

        public SpecificationBuilder(Specification<TEntity, TKey, TResult> specification) : base(specification) => Specification = specification;
    }

    public class SpecificationBuilder<TEntity> : ISpecificationBuilder<TEntity>
    {
        public Specification<TEntity> Specification { get; }

        public SpecificationBuilder(Specification<TEntity> specification) => Specification = specification;
    }
}
