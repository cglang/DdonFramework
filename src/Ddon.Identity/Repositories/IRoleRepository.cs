using Ddon.Domain.Entities.Identity;
using Ddon.Domain.Repositories;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IRoleRepository<TKey> : IRepository<Role<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
