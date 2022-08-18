using Ddon.Domain.Entities;
using Ddon.Domain.Entities.Identity;
using System;

namespace Ddon.Domain.UserInfo
{
    public class CurrentUserInfoAccessor<TKey> : ICurrentUserInfoAccessor<TKey> where TKey : IEquatable<TKey>
    {
        public User<TKey>? User { get; set; }

        public Tenant<TKey> Tenant { get; set; } = new Tenant<TKey>();

        public void Init(User<TKey>? user, Tenant<TKey>? tenant)
        {
            User = user;
            Tenant = tenant ?? new Tenant<TKey>();
        }
    }
}
