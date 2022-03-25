using Ddon.Domain.UserInfo;
using Ddon.Identity.Jwt;
using Ddon.Identity.Manager;
using Ddon.Repositiry.EntityFrameworkCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Identity.Extensions
{
    /// <summary>
    /// 添加
    /// </summary>
    public static class ServiceIdentityExtensions
    {
        /// <summary>
        /// 添加身份服务
        /// </summary>
        /// <param name="services"></param>
        public static void AddDdonIdentity<TDbContext, TKey>(this IServiceCollection services)
            where TDbContext : DbContext
            where TKey : IEquatable<TKey>
        {
            services.AddTransient<IIdentityManager<TKey>, IdentityManager<TKey>>();

            services.AddTransient<IRoleClaimRepository<TKey>, RoleClaimRepository<TDbContext, TKey>>();
            services.AddTransient<IRoleRepository<TKey>, RoleRepository<TDbContext, TKey>>();
            services.AddTransient<IUserClaimRepository<TKey>, UserClaimRepository<TDbContext, TKey>>();
            services.AddTransient<IUserRepository<TKey>, UserRepository<TDbContext, TKey>>();
            services.AddTransient<IUserRoleRepository<TKey>, UserRoleRepository<TDbContext, TKey>>();
            services.AddTransient<ITenantRepository<TKey>, TenantRepository<TDbContext, TKey>>();

            services.AddSingleton<ITokenTools<TKey>, TokenTools<TKey>>();

            services.AddScoped<ICurrentUserInfoAccessor<TKey>, CurrentUserInfoAccessor<TKey>>();
        }
    }
}
