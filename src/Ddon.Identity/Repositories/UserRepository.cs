using Ddon.Identity;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public class UserRepository<TDbContext, TKey> : EfCoreRepository<TDbContext, User<TKey>, TKey>, IUserRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IRoleRepository<TKey> _roleRepository;

        public UserRepository(TDbContext dbContext, IRoleRepository<TKey> roleRepository) : base(dbContext)
        {
            _roleRepository = roleRepository;
        }

        public async Task<User<TKey>?> GetUserAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var user = await FirstOrDefaultAsync(id, cancellationToken);
            if (user is not null)
            {
                var query = from tUserRoles in DbContext.UserRoles
                            join tRoles in DbContext.Roles on tUserRoles.RoleId equals tRoles.Id
                            where tUserRoles.Equals(user.Id)
                            select tRoles;
                var roles = await query.ToListAsync(cancellationToken);

                user.UserRoles = await _roleRepository.BindRolesPermissionAsync(query.ToList(), cancellationToken);

                var userPermissionsQuery = from tPermissions in DbContext.PermissionGrant
                                           where tPermissions.Type == PermissionGrantType.User && tPermissions.GrantKey.Equals(user.Id)
                                           select tPermissions;
                var userPermissions = await userPermissionsQuery.ToListAsync(cancellationToken);

                user.UserPermissions = user.UserRoles
                    .Where(x => x.RolePermissions is not null)
                    .SelectMany(x => x.RolePermissions!)
                    .ToList();

                user.UserPermissions.AddRange(userPermissions);
            }

            return user;
        }
    }
}
