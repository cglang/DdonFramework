using Ddon.Domain.Repositories;
using Ddon.Identity.Entities;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IUserRoleRepository<TKey> : IRepository<UserRole<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
