using Ddon.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class RoleRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, Role<TKey>, TKey>, IRoleRepository<TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public RoleRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
