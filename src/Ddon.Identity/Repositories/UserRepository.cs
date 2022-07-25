using Ddon.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class UserRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, User<TKey>, TKey>, IUserRepository<TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public UserRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
