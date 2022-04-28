using Ddon.Core.Services.LazyService;
using Ddon.Domain.Entities;
using Ddon.Domain.Entities.Identity;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.Identity
{
    public class IdentityDbContext<TDbContext, TKey> : BasicDbContext<TDbContext, TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public DbSet<User<TKey>>? Users { get; set; }

        public DbSet<Role<TKey>>? Roles { get; set; }

        public DbSet<UserRole<TKey>>? UserRoles { get; set; }

        public DbSet<UserClaim<TKey>>? UserClaims { get; set; }

        public DbSet<RoleClaim<TKey>>? RoleClaims { get; set; }

        public DbSet<UserToken<TKey>>? UserTokens { get; set; }

        public DbSet<UserLogin<TKey>>? UserLogins { get; set; }

        public DbSet<Tenant<TKey>>? Tenants { get; set; }

        public DbSet<PermissionGrant<TKey>>? PermissionGrant { get; set; }

        public IdentityDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<TDbContext> options) : base(lazyServiceProvider, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User<TKey>>(b => { b.ToTable("AppUser"); });
            modelBuilder.Entity<Role<TKey>>(b => { b.ToTable("AppRole"); });
            modelBuilder.Entity<UserRole<TKey>>(b => { b.ToTable("AppUserRole"); });
            modelBuilder.Entity<UserClaim<TKey>>(b => { b.ToTable("AppUserClaim"); });
            modelBuilder.Entity<RoleClaim<TKey>>(b => { b.ToTable("AppRoleClaim"); });
            modelBuilder.Entity<UserToken<TKey>>(b => { b.ToTable("AppUserToken"); });
            modelBuilder.Entity<UserLogin<TKey>>(b => { b.ToTable("AppUserLogin"); });
            modelBuilder.Entity<Tenant<TKey>>(b => { b.ToTable("AppTenant"); });
            modelBuilder.Entity<PermissionGrant<TKey>>(b => { b.ToTable("AppPermissionGrant"); });
        }
    }
}
