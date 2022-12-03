using Ddon.Core.Services.LazyService;
using Ddon.Identity.Entities;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Identity
{
    public class IdentityDbContext<TDbContext, TKey> : BasicDbContext<TDbContext>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public DbSet<User<TKey>> Users { get; set; }

        public DbSet<Role<TKey>> Roles { get; set; }

        public DbSet<UserRole<TKey>> UserRoles { get; set; }

        public DbSet<PermissionGrant<TKey>> PermissionGrant { get; set; }

        public IdentityDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<TDbContext> options) : base(lazyServiceProvider, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User<TKey>>(build => { build.ToTable("AppUser"); });
            modelBuilder.Entity<Role<TKey>>(build => { build.ToTable("AppRole"); });
            modelBuilder.Entity<UserRole<TKey>>(build => { build.ToTable("AppUserRole"); });
            modelBuilder.Entity<PermissionGrant<TKey>>(build => { build.ToTable("AppPermissionGrant"); });
        }
    }
}
