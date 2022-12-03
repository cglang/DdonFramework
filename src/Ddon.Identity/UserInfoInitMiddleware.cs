using Ddon.Domain.UserInfo;
using Ddon.Repositiry.EntityFrameworkCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Identity
{
    public class UserInfoInitMiddleware<TKey> : IMiddleware where TKey : IEquatable<TKey>
    {
        private readonly ICurrentUserInfoAccessor<TKey> _userinfoAccessor;
        private readonly IUserRepository<TKey> _userRepositity;

        public UserInfoInitMiddleware(ICurrentUserInfoAccessor<TKey> userinfoAccessor, IUserRepository<TKey> identityManager)
        {
            _userinfoAccessor = userinfoAccessor;
            _userRepositity = identityManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await _userRepositity.FirstOrDefaultAsync(x => x.Id.ToString() == userId);
                _userinfoAccessor!.Init(user);
            }
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