using Ddon.Cache;
using Ddon.Core;
using Ddon.Domain.UserInfo;
using Ddon.Identity.Manager;
using Ddon.Identity.Options;
using Ddon.Identity.Permission;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Ddon.Identity.Repositories;

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

            services.AddTransient<UserInfoInitMiddleware<TKey>>();

            services.AddTransient<IIdentityManager<TKey>, IdentityManager<TKey>>();

            services.AddTransient<IRoleRepository<TKey>, RoleRepository<TDbContext, TKey>>();
            services.AddTransient<IUserRepository<TKey>, UserRepository<TDbContext, TKey>>();
            services.AddTransient<IUserRoleRepository<TKey>, UserRoleRepository<TDbContext, TKey>>();
            services.AddTransient<IPermissionGrantRepository<TKey>, PermissionGrantRepository<TDbContext, TKey>>();

            services.AddScoped<ICurrentUserInfoAccessor<TKey>, CurrentUserInfoAccessor<TKey>>();

            var appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            services.AddSingleton(appSettings!);

            services.AddTransient<Auth<TKey>>();

            services.AddTransient<IPermissionDefinitionProvider, TPermissions>();
        }
    }
}
