using Ddon.Domain.Entities.Identity;
using Ddon.Domain.Repository;
using System;

namespace Ddon.Repositiry.EntityFrameworkCore.Identity
{
    public interface IUserRepository<TKey> : IRepository<User<TKey>, TKey> where TKey : IEquatable<TKey>
    {
    }
}
