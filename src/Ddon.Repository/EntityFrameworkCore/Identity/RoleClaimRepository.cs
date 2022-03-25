using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class RoleClaimRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, RoleClaim<TKey>, TKey>, IRoleClaimRepository<TKey>
        // EfCoreRepository<TDbContext, RoleClaim<TKey>, TKey>, IRoleClaimRepository<TKey>
        // EfCoreRepository<GardenerDbContext, TestEntity, Guid>, ITestRepository
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public RoleClaimRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
