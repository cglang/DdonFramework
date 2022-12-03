using Ddon.Identity.Entities;
using System;

namespace Ddon.Domain.UserInfo
{
    public class CurrentUserInfoAccessor<TKey> : ICurrentUserInfoAccessor<TKey> where TKey : IEquatable<TKey>
    {
        public User<TKey>? User { get; set; }

        public void Init(User<TKey>? user)
        {
            User = user;
        }
    }
}
