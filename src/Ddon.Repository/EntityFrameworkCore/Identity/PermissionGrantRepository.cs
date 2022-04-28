using Ddon.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class PermissionGrantRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, PermissionGrant<TKey>, TKey>, IPermissionGrantRepository<TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public PermissionGrantRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
