using Ddon.Identity;
using Ddon.Identity.Entities;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class PermissionGrantRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, PermissionGrant<TKey>, TKey>, IPermissionGrantRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        public PermissionGrantRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
