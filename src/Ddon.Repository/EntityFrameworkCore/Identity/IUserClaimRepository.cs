using Ddon.Domain.Repository;
using Ddon.Identity.Entities;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IUserClaimRepository<TKey> : IRepository<UserClaim<TKey>, TKey> where TKey : IEquatable<TKey>
    {

    }
}
