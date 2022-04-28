using Ddon.Cache;
using Ddon.Core;
using Ddon.Core.Utility;
using Ddon.Domain.Entities;
using Ddon.Domain.Entities.Identity;
using Ddon.Domain.Exceptions;
using Ddon.Identity.Jwt;
using Ddon.Identity.Manager.Dtos;
using Ddon.Identity.Permission;
using Ddon.Repositiry.EntityFrameworkCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Identity.Manager
{
    public class IdentityManager<TKey> : IIdentityManager<TKey> where TKey : IEquatable<TKey>
    {
        private readonly IPermissionDefinitionContext _permissionDefinitionContext;
        private readonly IUserRepository<TKey> _userRepository;
        private readonly IRoleRepository<TKey> _roleRepository;
        private readonly IRoleClaimRepository<TKey> _roleClaimRepository;
        private readonly IUserClaimRepository<TKey> _userClaimRepository;
        private readonly IUserRoleRepository<TKey> _userRoleRepository;
        private readonly ITenantRepository<TKey> _tenantRepository;
        private readonly ITokenTools<TKey> _tokenTools;
        private readonly IPermissionGrantRepository<TKey> _permissionGrantRepository;
        private readonly ICache _cache;

        public IQueryable<User<TKey>> Users => _userRepository.Query;
        public IQueryable<Role<TKey>> Roles => _roleRepository.Query;
        public IQueryable<UserClaim<TKey>> UserClaims => _userClaimRepository.Query;
        public IQueryable<RoleClaim<TKey>> RoleClaims => _roleClaimRepository.Query;
        public IQueryable<UserRole<TKey>> UserRoles => _userRoleRepository.Query;
        // TODO: 这两个的仓储还未实现
        public IQueryable<UserToken<TKey>> UserTokens => default!;
        public IQueryable<UserLogin<TKey>> UserLogins => default!;
        public IQueryable<Tenant<TKey>> Tenants => _tenantRepository.Query;

        public IdentityManager(
            IPermissionDefinitionContext permissionDefinitionContext,
            IUserRepository<TKey> userRepository,
            IRoleRepository<TKey> roleRepository,
            IRoleClaimRepository<TKey> roleClaimRepository,
            IUserClaimRepository<TKey> userClaimRepository,
            IUserRoleRepository<TKey> userRoleRepository,
            ITenantRepository<TKey> tenantRepository,
            IPermissionGrantRepository<TKey> permissionGrantRepository,
            ITokenTools<TKey> tokenTools,
            ICache cache)
        {
            _permissionDefinitionContext = permissionDefinitionContext;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _roleClaimRepository = roleClaimRepository;
            _userClaimRepository = userClaimRepository;
            _userRoleRepository = userRoleRepository;
            _tenantRepository = tenantRepository;
            _tokenTools = tokenTools;
            _permissionGrantRepository = permissionGrantRepository;
            _cache = cache;
        }

        public async Task<TokenDto> GenerateJwtTokenAsync(User<TKey> user)
        {
            return await _tokenTools.GenerateJwtTokenAsync(user);
        }

        public async Task<TokenDto> AccessTokenAsync(AccessTokenInPutDto input)
        {
            var existingUser = await Users.FirstOrDefaultAsync(x => x.UserName.Equals(input.UserName));
            if (existingUser == null)
            {
                return new TokenDto()
                {
                    Errors = new[] { "用户不存在!" },
                };
            }
            var isCorrect = await CheckPasswordAsync(input.UserName, input.Password);
            if (!isCorrect)
            {
                return new TokenDto()
                {
                    Errors = new[] { "用户名或密码错误!" },
                };
            }

            await CacheUserPermissions(existingUser);
            await CacheTenantInfo(existingUser);

            return await _tokenTools.GenerateJwtTokenAsync(existingUser);
        }

        public async Task<TokenDto> RefreshTokenAsync(RefreshTokenInPutDto input)
        {
            var claimsPrincipal = _tokenTools.GetClaimsPrincipalByToken(input.AccessToken);
            if (claimsPrincipal == null)
            {
                return new TokenDto()
                {
                    Errors = new[] { "无效的 AccessToken" },
                };
            }

            var expiryDateUnix = long.Parse(claimsPrincipal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateTimeUtc = UnixTime.UnixTimeStampToDateTime(expiryDateUnix);
            if (expiryDateTimeUtc > DateTime.Now)
            {
                return new TokenDto()
                {
                    Errors = new[] { "AccessToken 未过期" },
                };
            }

            var jti = claimsPrincipal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _cache.GetAsync<RefreshToken<TKey>>($"{CacheKey.RefreshTokenKey}{input.RefreshToken}");
            if (storedRefreshToken == null)
            {
                return new TokenDto()
                {
                    Errors = new[] { "无效的 RefreshToken" },
                };
            }

            if (storedRefreshToken.ExpiryTime < DateTime.Now)
            {
                return new TokenDto()
                {
                    Errors = new[] { "RefreshToken 已过期" },
                };
            }

            if (storedRefreshToken.Invalidated)
            {
                return new TokenDto()
                {
                    Errors = new[] { "RefreshToken 已失效" },
                };
            }

            if (storedRefreshToken.Used)
            {
                await _cache.RemoveAsync(storedRefreshToken.Token);
            }

            if (storedRefreshToken.JwtId != jti)
            {
                return new TokenDto()
                {
                    Errors = new[] { "RefreshToken 与 AccessToken 不匹配" },
                };
            }

            var dbUser = await Users.FirstOrDefaultAsync(x => x.Id.Equals(storedRefreshToken.UserId));
            return await GenerateJwtTokenAsync(dbUser!);
        }

        public async Task<bool> CheckPasswordAsync(string userName, string password)
        {
            var hash = Encryptor.MD5Hash(password);
            return await _userRepository.AnyAsync(u => u.UserName == userName && u.PasswordHash.Equals(hash));
        }

        public async Task<User<TKey>> CreateUserAsync(string userName, string password)
        {
            var hash = Encryptor.MD5Hash(password);
            var user = new User<TKey>
            {
                UserName = userName,
                PasswordHash = hash
            };
            await _userRepository.AddAsync(user, true);
            return user;
        }

        public async Task<Role<TKey>> CreateRoleAsync(string name)
        {
            var role = new Role<TKey> { Name = name };
            await _roleRepository.AddAsync(role, true);
            return role;
        }

        public async Task<User<TKey>?> GetUserByClaimsAsync(ClaimsPrincipal? user)
        {
            if (user is null) return null;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId.IsNullOrWhiteSpace()) return null;

            return await _userRepository.FirstOrDefaultAsync(x => x.Id.ToString() == userId);
        }

        public async Task<User<TKey>?> GetUserByIdAsync(TKey userId)
        {
            return await _userRepository.FirstOrDefaultAsync(x => x.Id.Equals(userId));
        }

        public async Task<Tenant<TKey>> GetUserTenantByClaimsAsync(ClaimsPrincipal? userClaims)
        {
            var user = await GetUserByClaimsAsync(userClaims);

            if (user is null) return new Tenant<TKey>();

            return await _tenantRepository.FirstOrDefaultAsync(x => x.Id.Equals(user.TenantId)) ?? new Tenant<TKey>();
        }

        public async Task<Tenant<TKey>> GetUserTenantByUserIdAsync(TKey id)
        {
            var user = await Users.FirstOrDefaultAsync(x => x.Id.Equals(id));

            if (user is null)
            {
                return new Tenant<TKey>();
            }

            return await _tenantRepository.FirstOrDefaultAsync(x => x.Id.Equals(user.TenantId)) ?? new Tenant<TKey>();
        }

        public async Task AddUserClaimAsync(TKey userId, string permissionName)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            if (user == null)
                throw new ApplicationServiceException("没有要添加此权限的用户");

            var have = await _permissionGrantRepository.AnyAsync(x => x.Name.Equals(permissionName) && x.GrantKey.Equals(userId));
            var existPermission = _permissionDefinitionContext.GetPermissionOrNull(permissionName) != null;

            if (!existPermission || have)
                throw new ApplicationServiceException("没有此权限的定义或该用户已经用此权限");

            await _permissionGrantRepository.AddAsync(new PermissionGrant<TKey>
            {
                GrantKey = userId,
                Name = permissionName,
                Type = PermissionGrantType.User
            }, true);
        }

        public async Task RemoveUserClaimAsync(TKey userId, string permissionName)
        {
            var permission = await _permissionGrantRepository.FirstOrDefaultAsync(x => x.GrantKey.Equals(userId) && x.Name.Equals(permissionName));
            if (permission == null)
            {
                throw new ApplicationServiceException("没有未此角色赋予此权限");
            }
            await _permissionGrantRepository.DeleteAsync(permission, true);
        }

        public async Task AddRoleClaimAsync(TKey roleId, string permissionName)
        {
            var role = await _roleRepository.FirstOrDefaultAsync(x => x.Id.Equals(roleId));
            if (role == null)
                throw new ApplicationServiceException("没有此要添加权限的角色");

            var have = await _permissionGrantRepository.AnyAsync(x => x.Name.Equals(permissionName) && x.GrantKey.Equals(roleId));
            var existPermission = _permissionDefinitionContext.GetPermissionOrNull(permissionName) != null;

            if (!existPermission || have)
                throw new ApplicationServiceException("没有此权限的定义或该角色已经用此权限");

            await _permissionGrantRepository.AddAsync(new PermissionGrant<TKey>
            {
                GrantKey = roleId,
                Name = permissionName,
                Type = PermissionGrantType.Role
            }, true);
        }

        public async Task RemoveRolePermissionAsync(TKey roleId, string permissionName)
        {
            var permission = await _permissionGrantRepository.FirstOrDefaultAsync(x => x.GrantKey.Equals(roleId) && x.Name.Equals(permissionName));
            if (permission == null)
            {
                throw new ApplicationServiceException("没有未改角色赋予此权限");
            }

            await _permissionGrantRepository.DeleteAsync(permission, true);
        }

        public async Task DeleteRoleAsync(TKey id)
        {
            var entity = await _roleRepository.FirstOrDefaultAsync(x => x.Id.Equals(id));

            if (entity is null)
                throw new ApplicationServiceException("没有此Id的记录");

            await _roleRepository.DeleteAsync(entity, true);

            // 移除赋予用户的角色 移除该角色的权限
            await _userRoleRepository.DeleteAsync(x => x.RoleId.Equals(id));
            await _permissionGrantRepository.DeleteAsync(x => x.GrantKey.Equals(id) && x.Type.Equals(PermissionGrantType.Role));
        }

        public async Task<Role<TKey>> UpdateRoleAsync(Role<TKey> entity)
        {
            var roleEntity = await _roleRepository.FirstOrDefaultAsync(x => x.Id.Equals(entity.Id));
            if (roleEntity is null)
            {
                throw new ApplicationServiceException("没有此角色");
            }

            roleEntity.Name = entity.Name;
            roleEntity.NormalizedName = entity.NormalizedName;
            roleEntity.ConcurrencyStamp = entity.ConcurrencyStamp;
            await _roleRepository.UpdateAsync(roleEntity, true);
            return roleEntity;
        }

        /// <summary>
        /// 设置用户的权限信息到缓存
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task CacheUserPermissions(User<TKey> user)
        {
            var userRoles = await _userRoleRepository.GetListAsync(x => x.UserId.Equals(user.Id));
            var userRoleIds = userRoles.Select(x => x.RoleId);
            var permissions = await _permissionGrantRepository.GetListAsync(x =>
               (userRoleIds.Contains(x.GrantKey) && x.Type == PermissionGrantType.Role) ||
               (x.GrantKey.Equals(user.Id) && x.Type == PermissionGrantType.User));

            var permissionText = permissions.Select(x => x.Name).ToList();

            await _cache.SetAsync($"{CacheKey.UserClaimsKey}{user.Id}", permissionText);
        }

        /// <summary>
        /// 缓存用户租户信息
        /// </summary>
        private async Task CacheTenantInfo(User<TKey> existingUser)
        {
            var tenant = await _cache.GetAsync<Tenant<TKey>>($"{CacheKey.TenantKey}{existingUser.Id}");

            if (tenant is null)
            {
                tenant = await GetUserTenantByUserIdAsync(existingUser.Id);
                await _cache.SetAsync($"{CacheKey.TenantKey}{existingUser.Id}", tenant);
            }
        }
    }
}
