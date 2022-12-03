using Ddon.Identity;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class RoleRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, Role<TKey>, TKey>, IRoleRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IPermissionGrantRepository<TKey> _permissionGrantRepository;

        public RoleRepository(TDbContext dbContext, IPermissionGrantRepository<TKey> permissionGrantRepository) : base(dbContext)
        {
            _permissionGrantRepository = permissionGrantRepository;
        }

        public async Task<Role<TKey>?> GetRoleAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var role = await FirstOrDefaultAsync(id, cancellationToken);
            if (role is not null)
            {
                role.RolePermissions = await _permissionGrantRepository.GetListAsync(x => x.GrantKey.Equals(role.Id) && x.Type == PermissionGrantType.Role);
            }

            return role;
        }

        public async Task<List<Role<TKey>>> GetRolesAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
        {
            var roles = await GetListAsync(x => ids.Contains(x.Id), cancellationToken);

            if (roles.Any())
            {
                var query = from tPermissions in DbContext.PermissionGrant
                            where tPermissions.Type == PermissionGrantType.Role && ids.Contains(tPermissions.GrantKey)
                            group tPermissions by tPermissions.GrantKey;
                var groupRolePermissions = await query.ToListAsync(cancellationToken);

                roles.ForEach(x => x.RolePermissions = groupRolePermissions.FirstOrDefault(g => g.Key.Equals(x.Id))?.ToList());
            }

            return roles;
        }

        public async Task<List<Role<TKey>>> GetRolesAsync(Expression<Func<Role<TKey>, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var roles = await GetListAsync(predicate, cancellationToken);
            if (roles.Any())
            {
                var ids = roles.Select(x => x.Id);
                var query = from tPermissions in DbContext.PermissionGrant
                            where tPermissions.Type == PermissionGrantType.Role && ids.Contains(tPermissions.GrantKey)
                            group tPermissions by tPermissions.GrantKey;
                var groupRolePermissions = await query.ToListAsync(cancellationToken);

                roles.ForEach(x => x.RolePermissions = groupRolePermissions.FirstOrDefault(g => g.Key.Equals(x.Id))?.ToList());
            }

            return roles;
        }

        public async Task<List<Role<TKey>>> BindRolesPermissionAsync(List<Role<TKey>> roles, CancellationToken cancellationToken = default)
        {
            if (roles.Any())
            {
                var ids = roles.Select(x => x.Id);
                var query = from tPermissions in DbContext.PermissionGrant
                            where tPermissions.Type == PermissionGrantType.Role && ids.Contains(tPermissions.GrantKey)
                            group tPermissions by tPermissions.GrantKey;
                var groupRolePermissions = await query.ToListAsync(cancellationToken);

                roles.ForEach(x => x.RolePermissions = groupRolePermissions.FirstOrDefault(g => g.Key.Equals(x.Id))?.ToList());
            }

            return roles;
        }
    }
}
