using Ddon.Domain.Repositories;
using Ddon.Identity.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IUserRepository<TKey> : IRepository<User<TKey>, TKey> where TKey : IEquatable<TKey>
    {
        Task<User<TKey>?> GetUserAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
