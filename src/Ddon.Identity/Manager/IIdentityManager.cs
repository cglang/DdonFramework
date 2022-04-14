using Ddon.Domain.Entities;
using Ddon.Identity.Entities;
using Ddon.Identity.Manager.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ddon.Identity.Manager
{
    public interface IIdentityManager<TKey> where TKey : IEquatable<TKey>
    {
        DbSet<User<TKey>> Users { get; }

        DbSet<Role<TKey>> Roles { get; }

        DbSet<UserClaim<TKey>> UserClaims { get; }

        DbSet<RoleClaim<TKey>> RoleClaims { get; }

        DbSet<UserRole<TKey>> UserRoles { get; }

        DbSet<UserToken<TKey>> UserTokens { get; }

        DbSet<UserLogin<TKey>> UserLogins { get; }

        DbSet<Tenant<TKey>> Tenants { get; }

        Task<TokenDto> AccessTokenAsync(AccessTokenInPutDto input);

        Task<TokenDto> RefreshTokenAsync(RefreshTokenInPutDto input);

        Task<TokenDto> GenerateJwtTokenAsync(User<TKey> user);

        Task<bool> CheckPasswordAsync(string userName, string password);

        Task<User<TKey>> CreateUserAsync(string userName, string password);

        Task RemoveRoleClaimAsync(TKey roleId, string permissionName);

        Task<User<TKey>?> GetUserByClaimsAsync(ClaimsPrincipal? user);

        Task<Tenant<TKey>> GetUserTenantByClaimsAsync(ClaimsPrincipal? user);

        Task<Tenant<TKey>> GetUserTenantByClaimsAsync(TKey id);

        Task AddUserClaimAsync(TKey userId, string permissionName);

        Task RemoveUserClaimAsync(TKey userId, string permissionName);

        Task AddRoleClaimAsync(TKey roleId, string permissionName);

        Task DeleteRoleAsync(TKey id);

        Task<Role<TKey>> UpdateAsync(Role<TKey> entity);

        Task<Role<TKey>> CreateRoleAsync(string name);
    }
}
