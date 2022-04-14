using Ddon.Domain.Repository;
using Ddon.Identity.Entities;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IRoleRepository<TKey> : IRepository<Role<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
