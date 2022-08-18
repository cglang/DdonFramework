using Ddon.Domain.Entities.Identity;
using Ddon.Domain.Repositories;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IPermissionGrantRepository<TKey> : IRepository<PermissionGrant<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
