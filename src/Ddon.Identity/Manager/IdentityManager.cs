using Ddon.Core;
using Ddon.Domain.Exceptions;
using Ddon.Identity.Entities;
using Ddon.Identity.Permission;
using System;
using System.Threading.Tasks;
using Ddon.Identity.Repositories;
using Ddon.Repository.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Manager
{
    public class IdentityManager<TDbContext, TKey> : IIdentityManager<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly TDbContext _dbContext;
        private readonly IPermissionDefinitionContext _permissionDefinitionContext;
        private readonly IUserRepository<TKey> _userRepository;
        private readonly IRoleRepository<TKey> _roleRepository;
        private readonly IUserRoleRepository<TKey> _userRoleRepository;
        private readonly IPermissionGrantRepository<TKey> _permissionGrantRepository;

        public IdentityManager(
            TDbContext dbContext,
            IPermissionDefinitionContext permissionDefinitionContext,
            IUserRepository<TKey> userRepository,
            IRoleRepository<TKey> roleRepository,
            IUserRoleRepository<TKey> userRoleRepository,
            IPermissionGrantRepository<TKey> permissionGrantRepository)
        {
            _dbContext = dbContext;
            _permissionDefinitionContext = permissionDefinitionContext;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _permissionGrantRepository = permissionGrantRepository;
        }

        public async Task<bool> CheckPasswordAsync(string userName, string password)
        {
            var hash = Encryptor.MD5Hash(password);
            return await _userRepository.User.AsNoTracking()
                .AnyAsync(u => u.UserName == userName && u.PasswordHash.Equals(hash));
        }

        public async Task<Role<TKey>> CreateRoleAsync(string name)
        {
            var role = new Role<TKey> { Name = name };
            await _roleRepository.Role.AddAsync(role);
            await _dbContext.SaveChangesAsync();
            return role;
        }

        public async Task AddUserPermissionAsync(TKey userId, string permissionName)
        {
            var user = await _userRepository.User.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            if (user == null)
                throw new ApplicationServiceException("没有要添加此权限的用户");

            var have = await _permissionGrantRepository.PermissionGrant.AnyAsync(x =>
                x.Name.Equals(permissionName) && x.GrantKey.Equals(userId));
            var existPermission = _permissionDefinitionContext.GetPermissionOrNull(permissionName) != null;

            if (!existPermission || have)
                throw new ApplicationServiceException("没有此权限的定义或该用户已经用此权限");

            await _permissionGrantRepository.PermissionGrant.AddAsync(
                new PermissionGrant<TKey>
                {
                    GrantKey = userId,
                    Name = permissionName,
                    Type = PermissionGrantType.User
                });

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveUserPermissionAsync(TKey userId, string permissionName)
        {
            var permission =
                await _permissionGrantRepository.PermissionGrant.FirstOrDefaultAsync(x =>
                    x.GrantKey.Equals(userId) && x.Name.Equals(permissionName));
            if (permission == null)
            {
                throw new ApplicationServiceException("没有未此角色赋予此权限");
            }

            await _permissionGrantRepository.PermissionGrant.DeleteAsync(permission.Id);

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddRolePermissionAsync(TKey roleId, string permissionName)
        {
            var role = await _roleRepository.Role.FirstOrDefaultAsync(x => x.Id.Equals(roleId));
            if (role == null)
                throw new ApplicationServiceException("没有此要添加权限的角色");

            var have = await _permissionGrantRepository.PermissionGrant.AnyAsync(x =>
                x.Name.Equals(permissionName) && x.GrantKey.Equals(roleId));
            var existPermission = _permissionDefinitionContext.GetPermissionOrNull(permissionName) != null;

            if (!existPermission || have)
                throw new ApplicationServiceException("没有此权限的定义或该角色已经用此权限");

            await _permissionGrantRepository.PermissionGrant.AddAsync(
                new PermissionGrant<TKey>
                {
                    GrantKey = roleId,
                    Name = permissionName,
                    Type = PermissionGrantType.Role
                });

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveRolePermissionAsync(TKey roleId, string permissionName)
        {
            var permission =
                await _permissionGrantRepository.PermissionGrant.FirstOrDefaultAsync(x =>
                    x.GrantKey.Equals(roleId) && x.Name.Equals(permissionName));
            if (permission == null)
            {
                throw new ApplicationServiceException("没有未改角色赋予此权限");
            }

            await _permissionGrantRepository.PermissionGrant.DeleteAsync(permission.Id);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(TKey id)
        {
            var entity = await _roleRepository.Role.FirstOrDefaultAsync(x => x.Id.Equals(id));

            if (entity is null)
                throw new ApplicationServiceException("没有此Id的记录");

            await _roleRepository.Role.DeleteAsync(entity.Id);

            // 移除赋予用户的角色 移除该角色的权限
            await _userRoleRepository.UserRoles.DeleteAsync<UserRole<TKey>, TKey>(x => x.RoleId.Equals(id));
            await _permissionGrantRepository.PermissionGrant.DeleteAsync<PermissionGrant<TKey>, TKey>(
                x => x.GrantKey.Equals(id) && x.Type.Equals(PermissionGrantType.Role));

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Role<TKey>> UpdateRoleAsync(Role<TKey> entity)
        {
            var roleEntity = await _roleRepository.Role.FirstOrDefaultAsync(x => x.Id.Equals(entity.Id));
            if (roleEntity is null)
            {
                throw new ApplicationServiceException("没有此角色");
            }

            roleEntity.Name = entity.Name;
            roleEntity.NormalizedName = entity.NormalizedName;
            roleEntity.ConcurrencyStamp = entity.ConcurrencyStamp;
            _roleRepository.Role.Update(roleEntity);

            await _dbContext.SaveChangesAsync();
            return roleEntity;
        }
    }
}
