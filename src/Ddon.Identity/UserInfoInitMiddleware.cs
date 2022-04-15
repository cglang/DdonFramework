using Ddon.Domain.UserInfo;
using Ddon.Identity.Manager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Identity
{
    public class UserInfoInitMiddleware<TKey> : IMiddleware where TKey : IEquatable<TKey>
    {
        private readonly ICurrentUserInfoAccessor<TKey> _userinfoAccessor;
        private readonly IIdentityManager<TKey> _identityManager;

        public UserInfoInitMiddleware(ICurrentUserInfoAccessor<TKey> userinfoAccessor, IIdentityManager<TKey> identityManager)
        {
            _userinfoAccessor = userinfoAccessor;
            _identityManager = identityManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var user = await _identityManager.GetUserByClaimsAsync(context.User);
            var tenant = await _identityManager.GetUserTenantByClaimsAsync(context.User);
            _userinfoAccessor!.Init(user, tenant);

            await next(context);
        }
    }

    public static class UserInfoInitApplicationBuilderExtensions
    {
        /// <summary>
        /// 用户信息初始化
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUserInfoInit<TKey>(this IApplicationBuilder app) where TKey : IEquatable<TKey>
        {
            return app.UseMiddleware<UserInfoInitMiddleware<TKey>>();
        }
    }
}