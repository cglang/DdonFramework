using Ddon.Domain.Entities;
using Ddon.Domain.Entities.Identity;
using System;

namespace Ddon.Domain.UserInfo
{
    public interface ICurrentUserInfoAccessor<TKey> where TKey : IEquatable<TKey>
    {
        User<TKey>? User { get; set; }

        Tenant<TKey> Tenant { get; set; }

        void Init(User<TKey>? user, Tenant<TKey>? tenant);
    }
}
