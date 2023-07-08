using System;
using Ddon.Cache;
using Ddon.Core;
using Ddon.Domain.UserInfo;
using Ddon.Identity.Entities;
using Ddon.Identity.Manager;
using Ddon.Identity.Options;
using Ddon.Identity.Permission;
using Ddon.Identity.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Identity
{
    public class IdentityModule<TDbContext, TKey, TPermissions> : Module
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
        where TPermissions : class, IPermissionDefinitionProvider
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CacheModule>(services, configuration);

            services.AddIdentityCore<User<TKey>>().AddRoles<Role<TKey>>().AddEntityFrameworkStores<TDbContext>();            

            services.AddScoped<IIdentityManager<TKey>, IdentityManager<TDbContext, TKey>>();

            services.AddScoped<IRoleRepository<TKey>, RoleRepository<TDbContext, TKey>>();
            services.AddScoped<IUserRepository<TKey>, UserRepository<TDbContext, TKey>>();
            services.AddScoped<IUserRoleRepository<TKey>, UserRoleRepository<TDbContext, TKey>>();
            services.AddScoped<IPermissionGrantRepository<TKey>, PermissionGrantRepository<TDbContext, TKey>>();

            services.AddScoped<ICurrentUserInfoAccessor<TKey>, CurrentUserInfoAccessor<TKey>>();

            // TODO: perf
            var appSettings = configuration.GetSection(nameof(IdentitySettings)).Get<IdentitySettings>();
            services.AddSingleton(appSettings!);

            services.AddScoped<Auth<TKey>>();

            services.AddSingleton<IPermissionDefinitionProvider, TPermissions>();
        }
    }
}
