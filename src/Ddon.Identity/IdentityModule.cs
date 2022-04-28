using Ddon.Cache;
using Ddon.Core;
using Ddon.Domain.UserInfo;
using Ddon.Identity.Jwt;
using Ddon.Identity.Manager;
using Ddon.Identity.Options;
using Ddon.Identity.Permission;
using Ddon.Repositiry.EntityFrameworkCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ddon.Identity
{
    public class IdentityModule<TDbContext, TKey, TPermissions> : Module
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
        where TPermissions : class, IPermissionDefinitionProvider
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CacheModule>(services, configuration);

            services.AddTransient<UserInfoInitMiddleware<TKey>>();

            services.AddTransient<IIdentityManager<TKey>, IdentityManager<TKey>>();

            services.AddTransient<IRoleClaimRepository<TKey>, RoleClaimRepository<TDbContext, TKey>>();
            services.AddTransient<IRoleRepository<TKey>, RoleRepository<TDbContext, TKey>>();
            services.AddTransient<IUserClaimRepository<TKey>, UserClaimRepository<TDbContext, TKey>>();
            services.AddTransient<IUserRepository<TKey>, UserRepository<TDbContext, TKey>>();
            services.AddTransient<IUserRoleRepository<TKey>, UserRoleRepository<TDbContext, TKey>>();
            services.AddTransient<ITenantRepository<TKey>, TenantRepository<TDbContext, TKey>>();
            services.AddTransient<IPermissionGrantRepository<TKey>, PermissionGrantRepository<TDbContext, TKey>>();            

            services.AddSingleton<ITokenTools<TKey>, TokenTools<TKey>>();

            services.AddScoped<ICurrentUserInfoAccessor<TKey>, CurrentUserInfoAccessor<TKey>>();

            var appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddTransient<Auth<TKey>>();

            services.AddTransient<IPermissionDefinitionProvider, TPermissions>();
        }
    }
}
