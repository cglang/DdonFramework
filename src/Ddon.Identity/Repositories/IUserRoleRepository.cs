using System;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Repositories
{
    public interface IUserRoleRepository<TKey> where TKey : IEquatable<TKey>
    {
        DbSet<UserRole<TKey>> UserRoles { get; }
    }
}
