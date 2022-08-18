using Ddon.Domain.Entities;
using Ddon.Domain.Repositories;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface ITenantRepository<TKey> : IRepository<Tenant<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
