using System;
using Ddon.Core.Exceptions;
using Ddon.Domain.Entities;
using Ddon.Identity.Entities;
using Microsoft.AspNetCore.Http;

namespace Ddon.Domain.UserInfo
{
    public class CurrentUserInfoAccessor<TKey> : ICurrentUserInfoAccessor<TKey>
        where TKey : IEquatable<TKey>
    {
        public User<TKey>? User { get; set; }

        public Tenant<TKey> Tenant { get; set; }

        public CurrentUserInfoAccessor(IHttpContextAccessor httpContext)
        {
            // TODO: 可从N多位置获取用户信息不只是 HttpContext

            var userClaims = httpContext.HttpContext?.User;
            if (userClaims is null) throw new UnauthorizedException("没有身份信息!");

            //User = identityManager.GetUserByClaimsAsync(userClaims).Result;
            //Tenant = identityManager.get(userClaims).Result;

            Tenant = new Tenant<TKey>();
        }
    }
}
