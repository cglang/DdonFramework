using Ddon.Domain.Entities;
using System;

namespace Ddon.Identity.Specifications.Builder
{
    public interface ISpecificationBuilder<T, TKey, TResult> : ISpecificationBuilder<T>
        where T : TenantEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        new Specification<T, TKey, TResult> Specification { get; }
    }

    public interface ISpecificationBuilder<T>
    {
        Specification<T> Specification { get; }
    }
}
