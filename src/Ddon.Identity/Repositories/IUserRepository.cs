using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Repositories
{
    public interface IUserRepository<TKey> where TKey : IEquatable<TKey>
    {
        DbSet<User<TKey>> User { get; }
        Task<User<TKey>?> GetUserAsync(TKey id, CancellationToken cancellationToken = default);
    }
}
