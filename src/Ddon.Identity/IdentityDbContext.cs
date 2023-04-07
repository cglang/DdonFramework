using System;
using Ddon.Core.Services.Guids;
using Ddon.Identity.Entities;
using Ddon.Repositiry;
using Ddon.Repository;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Identity
{
    public abstract class IdentityDbContext<TDbContext, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TPermissionGrant>
        : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>, IDbContextSoftDelete
        where TDbContext : DbContext
        where TUser : User<TKey>
        where TRole : Role<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : UserClaim<TKey>
        where TUserRole : UserRole<TKey>
        where TUserLogin : UserLogin<TKey>
        where TRoleClaim : RoleClaim<TKey>
        where TUserToken : UserToken<TKey>
        where TPermissionGrant : PermissionGrant<TKey>
    {
        public IdentityDbContext(IServiceProvider serviceProvider, DbContextOptions<TDbContext> options) : base(options)
        {
            if (serviceProvider is not null)
            {
                var guidGenerator = serviceProvider.GetRequiredService<IGuidGenerator>();
                BasicDbContext.Initialize(ChangeTracker, guidGenerator);
            }
        }

        public DbSet<PermissionGrant<TKey>> PermissionGrant { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TUser>(build => build.ToTable("AppUser"));
            modelBuilder.Entity<TRole>(build => build.ToTable("AppRole"));
            modelBuilder.Entity<TUserRole>(build => build.ToTable("AppUserRole"));
            modelBuilder.Entity<TUserClaim>(build => build.ToTable("AppUserClaim"));
            modelBuilder.Entity<TUserLogin>(build => build.ToTable("AppUserLogin"));
            modelBuilder.Entity<TRoleClaim>(build => build.ToTable("AppRoleClaim"));
            modelBuilder.Entity<TUserToken>(build => build.ToTable("AppUserToken"));
            modelBuilder.Entity<TPermissionGrant>(build => build.ToTable("AppPermissionGrant"));

            OnModleSoftDelete(modelBuilder);
        }

        public void OnModleSoftDelete(ModelBuilder modelBuilder)
        {
            DbContextSoftDelete.Builder(modelBuilder);
        }
    }

    public abstract class IdentityDbContext<TDbContext, TUser, TRole, TKey>
        : IdentityDbContext<TDbContext, TUser, TRole, TKey, UserClaim<TKey>, UserRole<TKey>, UserLogin<TKey>, RoleClaim<TKey>, UserToken<TKey>, PermissionGrant<TKey>>
        where TDbContext : DbContext
        where TUser : User<TKey>
        where TRole : Role<TKey>
        where TKey : IEquatable<TKey>
    {
        public IdentityDbContext(IServiceProvider serviceProvider, DbContextOptions<TDbContext> options) : base(serviceProvider, options)
        {
        }
    }

    public abstract class IdentityDbContext<TDbContext, TKey> : IdentityDbContext<TDbContext, User<TKey>, Role<TKey>, TKey>
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public IdentityDbContext(IServiceProvider serviceProvider, DbContextOptions<TDbContext> options) : base(serviceProvider, options)
        {
        }
    }
}
