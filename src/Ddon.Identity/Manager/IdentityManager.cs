using Ddon.Cache;
using Ddon.Core;
using Ddon.Core.Models;
using Ddon.Core.Services.Permission;
using Ddon.Core.Utility;
using Ddon.Domain.Entities;
using Ddon.Identity.Entities;
using Ddon.Identity.Jwt;
using Ddon.Identity.Manager.Dtos;
using Ddon.Repositiry.EntityFrameworkCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        private readonly ICache _cache;

        public DbSet<User<TKey>> Users => _userRepository.DbSet;
        public DbSet<Role<TKey>> Roles => _roleRepository.DbSet;
        public DbSet<UserClaim<TKey>> UserClaims => _userClaimRepository.DbSet;
        public DbSet<RoleClaim<TKey>> RoleClaims => _roleClaimRepository.DbSet;
        public DbSet<UserRole<TKey>> UserRoles => _userRoleRepository.DbSet;
        // TODO: 这两个的仓储还未实现
        public DbSet<UserToken<TKey>> UserTokens => default!;
        public DbSet<UserLogin<TKey>> UserLogins => default!;
        public DbSet<Tenant<TKey>> Tenants => _tenantRepository.DbSet;

        public IdentityManager(
            IPermissionDefinitionContext permissionDefinitionContext,
            IUserRepository<TKey> userRepository,
            IRoleRepository<TKey> roleRepository,
            IRoleClaimRepository<TKey> roleClaimRepository,
            IUserClaimRepository<TKey> userClaimRepository,
            IUserRoleRepository<TKey> userRoleRepository,
            ITenantRepository<TKey> tenantRepository,
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

            var cacheUserPermissions = SetUserPermissionsToCache(existingUser);
            var cacheTenantInfo = CacheTenantInfo(existingUser);
            Task.WaitAll(cacheUserPermissions, cacheTenantInfo);

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
            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                return new TokenDto()
                {
                    Errors = new[] { "AccessToken 未过期" },
                };
            }

            var jti = claimsPrincipal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = _cache.Get<RefreshToken<TKey>>($"{CacheKey.RefreshTokenKey}{input.RefreshToken}");
            if (storedRefreshToken == null)
            {
                return new TokenDto()
                {
                    Errors = new[] { "无效的 RefreshToken" },
                };
            }

            if (storedRefreshToken.ExpiryTime < DateTime.UtcNow)
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
                _cache.Remove(storedRefreshToken.Token);
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

        public async Task RemoveRoleClaimAsync(TKey roleId, string permissionName)
        {
            var role = await _roleClaimRepository.FirstOrDefaultAsync(x => x.RoleId.Equals(roleId) && permissionName.Equals(x.ClaimType));
            if (role == null)
            {
                throw new ApplicationException("没有此Claim");
            }

            await _roleClaimRepository.DeleteAsync(role, true);
        }

        public async Task<User<TKey>?> GetUserByClaimsAsync(ClaimsPrincipal? user)
        {
            if (user is null) return null;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId.IsNullOrWhiteSpace()) return null;

            return await _userRepository.FirstOrDefaultAsync(x => x.Id.ToString() == userId);
        }

        /// <summary>
        /// 使用当前相关用户信息创建一个租户对象,如没有用户信息则返回默认租户对象
        /// </summary>
        /// <param name="userClaims"></param>
        /// <returns></returns>
        public async Task<Tenant<TKey>> GetUserTenantByClaimsAsync(ClaimsPrincipal? userClaims)
        {
            var user = await GetUserByClaimsAsync(userClaims);

            if (user is null)
            {
                return new Tenant<TKey>();
            }

            return await _tenantRepository.FirstOrDefaultAsync(x => x.Id.Equals(user.TenantId)) ?? new Tenant<TKey>();
        }

        public async Task<Tenant<TKey>> GetUserTenantByClaimsAsync(TKey id)
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
            if (user != null)
            {

                var have = await _roleClaimRepository.AnyAsync(x => permissionName.Equals(x.ClaimType));
                var existPermission = _permissionDefinitionContext.GetPermissionOrNull(permissionName) != null;
                if (existPermission && !have)
                {
                    await _userClaimRepository.AddAsync(new UserClaim<TKey>
                    {
                        UserId = userId,
                        ClaimType = permissionName,
                        ClaimValue = permissionName
                    }, true);
                }
                else
                {
                    throw new ApplicationException("没有此权限的定义或该用户已经用此权限");
                }
            }
            else
            {
                throw new ApplicationException("没有此要添加权限的用户");
            }
        }

        public async Task RemoveUserClaimAsync(TKey userId, string permissionName)
        {
            var user = await _userClaimRepository.FirstOrDefaultAsync(x => x.UserId.Equals(userId) && permissionName.Equals(x.ClaimType));
            if (user == null)
            {
                throw new ApplicationException("没有此Claim");
            }
            await _userClaimRepository.DeleteAsync(user, true);
        }

        public async Task AddRoleClaimAsync(TKey roleId, string permissionName)
        {
            var role = await _roleRepository.FirstOrDefaultAsync(x => x.Id.Equals(roleId));
            if (role != null)
            {
                var have = await _roleClaimRepository.AnyAsync(x => permissionName.Equals(x.ClaimType));
                var existPermission = _permissionDefinitionContext.GetPermissionOrNull(permissionName) != null;
                if (existPermission && !have)
                {
                    await _roleClaimRepository.AddAsync(new RoleClaim<TKey>
                    {
                        RoleId = roleId,
                        ClaimType = permissionName,
                        ClaimValue = permissionName
                    }, true);
                }
                else
                {
                    throw new ApplicationException("没有此权限的定义或该角色已经用此权限");
                }
            }
            else
            {
                throw new ApplicationException("没有此要添加权限的角色");
            }
        }

        public async Task DeleteRoleAsync(TKey id)
        {
            var entity = await _roleRepository.FirstOrDefaultAsync(x => x.Id.Equals(id));

            if (entity is null)
            {
                throw new ApplicationException("没有此Id的记录");
            }

            await _roleRepository.DeleteAsync(entity, true);
        }

        public async Task<Role<TKey>> UpdateAsync(Role<TKey> entity)
        {
            var roleEntity = await _roleRepository.FirstOrDefaultAsync(x => x.Id.Equals(entity.Id));
            if (roleEntity is null)
            {
                throw new ApplicationException("没有此角色");
            }

            roleEntity.Name = entity.Name;
            roleEntity.NormalizedName = entity.NormalizedName;
            roleEntity.ConcurrencyStamp = entity.ConcurrencyStamp;

            await _roleRepository.SaveChangesAsync();

            return roleEntity;
        }


        /// <summary>
        /// 设置用户的权限信息到缓存
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task SetUserPermissionsToCache(User<TKey> user)
        {
            var userClaims = await UserClaims.Where(x => x.UserId.Equals(user.Id)).ToListAsync();
            var roleClaims = new List<RoleClaim<TKey>>();
            var roles = await UserRoles.Where(x => x.UserId.Equals(user.Id)).ToListAsync();

            foreach (var role in roles)
            {
                bool decide = _cache.Get($"{CacheKey.RoleClaimsKey}{role.RoleId}") is not null;
                if (decide)
                {
                    roleClaims.AddRange(await RoleClaims.Where(x => x.RoleId.Equals(role.RoleId)).ToListAsync());
                    _cache.Set($"{CacheKey.RoleClaimsKey}{role.RoleId}", roleClaims.Select(x => x.ClaimType));
                }
            }

            var claims = new List<string>();
            claims.AddRange(userClaims.Select(x => x.ClaimType));
            claims.AddRange(roleClaims.Select(x => x.ClaimType));
            _cache.Set($"{CacheKey.UserClaimsKey}{user.Id}", claims);
        }

        /// <summary>
        /// 缓存用户租户信息
        /// </summary>
        private async Task CacheTenantInfo(User<TKey> existingUser)
        {
            var tenant = _cache.Get<Tenant<TKey>>($"{CacheKey.TenantKey}{existingUser.Id}");

            if (tenant is null)
            {
                tenant = await GetUserTenantByClaimsAsync(existingUser.Id);
                _cache.Set($"{CacheKey.TenantKey}{existingUser.Id}", tenant);
            }
        }
    }
}
