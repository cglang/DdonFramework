using Ddon.Domain.Repository;
using Ddon.Identity.Entities;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IUserRepository<TKey> : IRepository<User<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
