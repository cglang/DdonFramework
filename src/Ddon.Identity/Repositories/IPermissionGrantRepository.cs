using System;
using Ddon.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ddon.Identity.Repositories
{
    public interface IPermissionGrantRepository<TKey> where TKey : IEquatable<TKey>
    {
        DbSet<PermissionGrant<TKey>> PermissionGrant { get; }
    }
}
