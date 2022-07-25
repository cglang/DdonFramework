using Ddon.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class UserClaimRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, UserClaim<TKey>, TKey>, IUserClaimRepository<TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public UserClaimRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
