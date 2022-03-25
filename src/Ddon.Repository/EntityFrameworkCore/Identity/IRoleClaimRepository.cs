using Ddon.Identity.Entities;
using Ddon.Identity.Repository;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IRoleClaimRepository<TKey> : IRepository<RoleClaim<TKey>, TKey> where TKey : IEquatable<TKey>
        //IRepository<RoleClaim<TKey>, TKey> where TKey : IEquatable<TKey>
        //IRepository<TestEntity, Guid>, ITransientDependency
    {
    }
}
