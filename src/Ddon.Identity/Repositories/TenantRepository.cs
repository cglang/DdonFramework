using Ddon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class TenantRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, Tenant<TKey>, TKey>, ITenantRepository<TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public TenantRepository(TDbContext dbContext) : base(dbContext)
        {
        }
    }
}
