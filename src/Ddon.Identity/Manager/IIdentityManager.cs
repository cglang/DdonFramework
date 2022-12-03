using Ddon.Identity.Entities;
using System;
using System.Threading.Tasks;

namespace Ddon.Identity.Manager
{
    public interface IIdentityManager<TKey> where TKey : IEquatable<TKey>
    {
        Task<bool> CheckPasswordAsync(string userName, string password);

        Task AddUserPermissionAsync(TKey userId, string permissionName);

        Task RemoveUserPermissionAsync(TKey userId, string permissionName);

        Task AddRolePermissionAsync(TKey roleId, string permissionName);

        Task RemoveRolePermissionAsync(TKey roleId, string permissionName);

        Task DeleteRoleAsync(TKey id);

        Task<Role<TKey>> UpdateRoleAsync(Role<TKey> entity);

        Task<Role<TKey>> CreateRoleAsync(string name);
    }
}
