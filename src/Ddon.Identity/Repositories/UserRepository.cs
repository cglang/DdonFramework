using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Ddon.Repository.Extensions;

namespace Ddon.Identity.Repositories
{
    public class UserRepository<TDbContext, TKey> : IUserRepository<TKey>
        where TDbContext : IdentityDbContext<TDbContext, TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly TDbContext _dbContext;
        private readonly IRoleRepository<TKey> _roleRepository;

        public DbSet<User<TKey>> User { get; }

        public UserRepository(TDbContext dbContext, IRoleRepository<TKey> roleRepository)
        {
            _dbContext = dbContext;
            _roleRepository = roleRepository;
            User = dbContext.Users;
        }


        public async Task<User<TKey>?> GetUserAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var user = await User.FirstOrDefaultAsync(id, cancellationToken);
            if (user is null) return user;

            var query = from tUserRoles in _dbContext.UserRoles
                join tRoles in _dbContext.Roles on tUserRoles.RoleId equals tRoles.Id
                where tUserRoles.RoleId.Equals(user.Id)
                select tRoles;

            var roles = await query.AsNoTracking().ToListAsync(cancellationToken);
            user.UserRoles = await _roleRepository.BindRolesPermissionAsync(roles, cancellationToken);

            var userPermissionsQuery = from tPermissions in _dbContext.PermissionGrant
                where tPermissions.Type == PermissionGrantType.User && tPermissions.GrantKey.Equals(user.Id)
                select tPermissions;
            var userPermissions = await userPermissionsQuery.ToListAsync(cancellationToken);

            user.UserPermissions = user.UserRoles
                .Where(x => x.RolePermissions is not null)
                .SelectMany(x => x.RolePermissions!)
                .ToList();

            user.UserPermissions.AddRange(userPermissions);

            return user;
        }
    }
}
