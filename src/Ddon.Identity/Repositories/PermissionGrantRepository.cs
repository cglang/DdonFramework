using System;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Repositories
{
    public class PermissionGrantRepository<TDbContext, TKey> : IPermissionGrantRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        public DbSet<PermissionGrant<TKey>> PermissionGrant { get; }

        public PermissionGrantRepository(TDbContext dbContext)
        {
            PermissionGrant = dbContext.PermissionGrant;
        }
    }
}
