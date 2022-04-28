using Ddon.Domain.Entities;
using Ddon.Domain.Entities.Identity;
using Ddon.Identity.Manager.Dtos;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Identity.Manager
{
    public interface IIdentityManager<TKey> where TKey : IEquatable<TKey>
    {
        IQueryable<User<TKey>> Users { get; }

        IQueryable<Role<TKey>> Roles { get; }

        IQueryable<UserClaim<TKey>> UserClaims { get; }

        IQueryable<RoleClaim<TKey>> RoleClaims { get; }

        IQueryable<UserRole<TKey>> UserRoles { get; }

        IQueryable<UserToken<TKey>> UserTokens { get; }

        IQueryable<UserLogin<TKey>> UserLogins { get; }

        IQueryable<Tenant<TKey>> Tenants { get; }

        Task<TokenDto> AccessTokenAsync(AccessTokenInPutDto input);

        Task<TokenDto> RefreshTokenAsync(RefreshTokenInPutDto input);

        Task<TokenDto> GenerateJwtTokenAsync(User<TKey> user);

        Task<bool> CheckPasswordAsync(string userName, string password);

        Task<User<TKey>> CreateUserAsync(string userName, string password);

        Task RemoveRolePermissionAsync(TKey roleId, string permissionName);

        Task<User<TKey>?> GetUserByClaimsAsync(ClaimsPrincipal? user);

        Task<User<TKey>?> GetUserByIdAsync(TKey userId);

        Task<Tenant<TKey>> GetUserTenantByClaimsAsync(ClaimsPrincipal? user);

        Task<Tenant<TKey>> GetUserTenantByUserIdAsync(TKey id);

        Task AddUserClaimAsync(TKey userId, string permissionName);

        Task RemoveUserClaimAsync(TKey userId, string permissionName);

        Task AddRoleClaimAsync(TKey roleId, string permissionName);

        Task DeleteRoleAsync(TKey id);

        Task<Role<TKey>> UpdateRoleAsync(Role<TKey> entity);

        Task<Role<TKey>> CreateRoleAsync(string name);
    }
}
