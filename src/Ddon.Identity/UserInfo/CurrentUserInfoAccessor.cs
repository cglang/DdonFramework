using System;
using Ddon.Identity.Entities;

namespace Ddon.Domain.UserInfo
{
    public class CurrentUserInfoAccessor<TKey> : ICurrentUserInfoAccessor<TKey> where TKey : IEquatable<TKey>
    {
        public User<TKey>? User { get; set; }
    }
}
