using Ddon.Domain.Repositories;
using Ddon.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IRoleRepository<TKey> : IRepository<Role<TKey>, TKey> where TKey : IEquatable<TKey>
    {
        Task<Role<TKey>?> GetRoleAsync(TKey id, CancellationToken cancellationToken = default);

        Task<List<Role<TKey>>> GetRolesAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

        Task<List<Role<TKey>>> GetRolesAsync(Expression<Func<Role<TKey>, bool>> predicate, CancellationToken cancellationToken = default);

        Task<List<Role<TKey>>> BindRolesPermissionAsync(List<Role<TKey>> roles, CancellationToken cancellationToken = default);
    }
}
