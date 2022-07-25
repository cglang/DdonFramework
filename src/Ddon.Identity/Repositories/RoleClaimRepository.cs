using Ddon.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class RoleClaimRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, RoleClaim<TKey>, TKey>, IRoleClaimRepository<TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public RoleClaimRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
