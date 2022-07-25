using Ddon.Domain.Entities.Identity;
using Ddon.Domain.Repository;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IRoleClaimRepository<TKey> : IRepository<RoleClaim<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
