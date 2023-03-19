using System;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Repositories
{
    public class UserRoleRepository<TDbContext, TKey> : IUserRoleRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        public DbSet<UserRole<TKey>> UserRoles { get; }

        public UserRoleRepository(TDbContext dbContext)
        {
            UserRoles = dbContext.UserRoles;
        }
    }
}
