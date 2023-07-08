using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ddon.Cache;
using Ddon.Core;
using Ddon.Domain.UserInfo;
using Ddon.Identity.Entities;
using Ddon.Identity.Exceptions;
using Ddon.Identity.Options;
using Microsoft.Extensions.Options;

namespace Ddon.Identity
{
    public class Auth<TKey> where TKey : IEquatable<TKey>
    {
        private readonly ICache _cache;
        private readonly ICurrentUserInfoAccessor<TKey> _userInfo;
        private readonly IdentitySettings _appSettings;

        public Auth(ICache cache, ICurrentUserInfoAccessor<TKey> userInfo, IOptions<IdentitySettings> options)
        {
            _cache = cache;
            _userInfo = userInfo;
            _appSettings = options.Value;
        }

        /// <summary>
        /// 权限验证
        /// </summary>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        public async Task CheckPermissionAsync(string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName)) return;

            if (_userInfo.User == null)
                throw new UnauthenticationException("用户未登录!");

            if (!await CheckPermissionAsync(_userInfo.User, permissionName))
                throw new UnauthorizedException($"用户没有权限:{permissionName}");
        }

        /// <summary>
        /// 验证权限的方法
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        private async Task<bool> CheckPermissionAsync(User<TKey>? user, string permissionName)
        {
            if (user is null) return false;

            // 系统管理员拥有所有权限            
            if (_appSettings.SystemAdminName.Equals(user.UserName)) return true;

            var userClaims = await _cache.GetAsync<List<string>>($"{CacheKey.UserKey}{user.Id}") ?? new();

            return userClaims.Any(x => x == permissionName);
        }
    }
}
