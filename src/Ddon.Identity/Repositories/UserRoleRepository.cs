using Ddon.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class UserRoleRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, UserRole<TKey>, TKey>, IUserRoleRepository<TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public UserRoleRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
