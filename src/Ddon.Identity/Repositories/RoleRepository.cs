using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Repositories
{
    public class RoleRepository<TDbContext, TKey> : IRoleRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly TDbContext _dbContext;
        private readonly IPermissionGrantRepository<TKey> _permissionGrantRepository;
        public DbSet<Role<TKey>> Role => _dbContext.Roles;

        public RoleRepository(TDbContext dbContext, IPermissionGrantRepository<TKey> permissionGrantRepository)
        {
            _dbContext = dbContext;
            _permissionGrantRepository = permissionGrantRepository;
        }


        public async Task<Role<TKey>?> GetRoleAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var role = await Role.AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
            if (role is not null)
            {
                role.RolePermissions = await _permissionGrantRepository.PermissionGrant.Where(x =>
                    x.GrantKey.Equals(role.Id) && x.Type == PermissionGrantType.Role).ToListAsync(cancellationToken);
            }

            return role;
        }

        public async Task<List<Role<TKey>>> GetRolesAsync(IEnumerable<TKey> ids,
            CancellationToken cancellationToken = default)
        {
            var roles = await Role.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

            if (roles.Any())
            {
                var query = from tPermissions in _dbContext.PermissionGrant
                    where tPermissions.Type == PermissionGrantType.Role && ids.Contains(tPermissions.GrantKey)
                    group tPermissions by tPermissions.GrantKey;
                var groupRolePermissions = await query.AsNoTracking().ToListAsync(cancellationToken);

                roles.ForEach(x =>
                    x.RolePermissions = groupRolePermissions.FirstOrDefault(g => g.Key.Equals(x.Id))?.ToList());
            }

            return roles;
        }

        public async Task<List<Role<TKey>>> GetRolesAsync(Expression<Func<Role<TKey>, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var roles = await Role.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
            if (roles.Any())
            {
                var ids = roles.Select(x => x.Id);
                var query = from tPermissions in _dbContext.PermissionGrant
                    where tPermissions.Type == PermissionGrantType.Role && ids.Contains(tPermissions.GrantKey)
                    group tPermissions by tPermissions.GrantKey;
                var groupRolePermissions = await query.AsNoTracking().ToListAsync(cancellationToken);

                roles.ForEach(x =>
                    x.RolePermissions = groupRolePermissions.FirstOrDefault(g => g.Key.Equals(x.Id))?.ToList());
            }

            return roles;
        }

        public async Task<List<Role<TKey>>> BindRolesPermissionAsync(List<Role<TKey>> roles,
            CancellationToken cancellationToken = default)
        {
            if (roles.Any())
            {
                var ids = roles.Select(x => x.Id);
                var query = from tPermissions in _dbContext.PermissionGrant
                    where tPermissions.Type == PermissionGrantType.Role && ids.Contains(tPermissions.GrantKey)
                    group tPermissions by tPermissions.GrantKey;
                var groupRolePermissions = await query.ToListAsync(cancellationToken);

                roles.ForEach(x =>
                    x.RolePermissions = groupRolePermissions.FirstOrDefault(g => g.Key.Equals(x.Id))?.ToList());
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            return roles;
        }
    }
}
