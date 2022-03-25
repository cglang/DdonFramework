using Ddon.Identity.Entities;
using Ddon.Identity.Repository;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IUserClaimRepository<TKey> : IRepository<UserClaim<TKey>, TKey> where TKey : IEquatable<TKey>
    {

    }
}
