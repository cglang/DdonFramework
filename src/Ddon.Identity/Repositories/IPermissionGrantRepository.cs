using Ddon.Domain.Repositories;
using Ddon.Identity.Entities;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IPermissionGrantRepository<TKey> : IRepository<PermissionGrant<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
