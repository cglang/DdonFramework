using AutoMapper;
using Ddon.Cache;
using Ddon.Core;
using Ddon.Core.Exceptions;
using Ddon.Core.Models;
using Ddon.Core.Services.Guids;
using Ddon.Core.Services.LazyService;
using Ddon.Domain.UserInfo;
using Ddon.Identity.Entities;
using Ddon.Identity.Manager;
using Ddon.Uow;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Application.Service
{
    public class ApplicationService<TKey> where TKey : IEquatable<TKey>
    {
        public ApplicationService(ILazyServiceProvider lazyServiceProvider)
        {
            LazyServiceProvider = lazyServiceProvider;
        }

        protected ILazyServiceProvider LazyServiceProvider { get; }

        /// <summary>
        /// 连续 Guid 生成
        /// </summary>
        protected IGuidGenerator GuidGenerator => LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>();

        /// <summary>
        /// Mapper 映射
        /// </summary>
        protected IMapper Mapper => LazyServiceProvider.LazyGetRequiredService<IMapper>();

        /// <summary>
        /// HttpContext 访问器
        /// </summary>
        protected IHttpContextAccessor HttpContextAccessor => LazyServiceProvider.LazyGetRequiredService<IHttpContextAccessor>();

        /// <summary>
        /// 缓存
        /// </summary>
        protected ICache Cache => LazyServiceProvider.LazyGetRequiredService<ICache>();

        /// <summary>
        /// Identity 仓储
        /// </summary>
        protected IIdentityManager<TKey> IdentityManager => LazyServiceProvider.LazyGetRequiredService<IIdentityManager<TKey>>();

        /// <summary>
        /// 工作单元
        /// </summary>
        protected IUnitOfWork UnitOfWork => LazyServiceProvider.LazyGetRequiredService<IUnitOfWork>();

        /// <summary>
        /// 本地化
        /// </summary>
        protected IStringLocalizer L => LazyServiceProvider.LazyGetRequiredService<IStringLocalizerFactory>().Create(null!);

        /// <summary>
        /// 基于 HTTP 请求的用户信息
        /// </summary>
        protected ICurrentUserInfoAccessor<TKey>? UserInfo => LazyServiceProvider.LazyGetRequiredService<ICurrentUserInfoAccessor<TKey>>();

        /// <summary>
        /// 权限验证
        /// </summary>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        protected async Task CheckPermissionAsync(string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                return;
            }

            // TODO:这里有可能会有多种客户端类型需要验证

            // 基于 http 的客户端
            var userClaims = HttpContextAccessor.HttpContext?.User;
            if (userClaims is null) throw new UnauthorizedException("没有身份信息!");

            var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId.IsNullOrWhiteSpace()) throw new UnauthorizedException("没有身份信息!");

            var user = await IdentityManager.GetUserByClaimsAsync(userClaims);
            if (user is null) throw new UnauthorizedException("没有这个用户!");

            if (!CheckPermission(user, permissionName))
            {
                throw new UnauthorizedException($"用户没有权限:{permissionName}");
            }
        }

        /// <summary>
        /// 验证权限的方法
        /// </summary>
        /// <param name="user"></param>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        private bool CheckPermission(User<TKey>? user, string permissionName)
        {
            if (user is null)
            {
                return false;
            }

            // 系统管理员拥有所有权限
            var appsettins = LazyServiceProvider.LazyGetRequiredService<AppSettings>();
            if (appsettins.SystemAdminName.Equals(user.UserName))
            {
                return true;
            }

            var userClaims = Cache.Get<List<string>>($"{CacheKey.UserKey}{user.Id}") ?? new();

            return userClaims.Any(x => x == permissionName);
        }
    }
}
