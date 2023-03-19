using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Repositories
{
    public interface IRoleRepository<TKey> where TKey : IEquatable<TKey>
    {
        DbSet<Role<TKey>> Role { get; }

        Task<Role<TKey>?> GetRoleAsync(TKey id, CancellationToken cancellationToken = default);

        Task<List<Role<TKey>>> GetRolesAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

        Task<List<Role<TKey>>> GetRolesAsync(Expression<Func<Role<TKey>, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<List<Role<TKey>>> BindRolesPermissionAsync(List<Role<TKey>> roles,
            CancellationToken cancellationToken = default);
    }
}
