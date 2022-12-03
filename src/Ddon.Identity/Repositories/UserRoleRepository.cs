using Ddon.Identity;
using Ddon.Identity.Entities;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class UserRoleRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, UserRole<TKey>, TKey>, IUserRoleRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        public UserRoleRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
