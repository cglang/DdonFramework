using System;
using Ddon.Domain.Entities;
using Ddon.Identity.Entities;

namespace Ddon.Domain.UserInfo
{
    public interface ICurrentUserInfoAccessor<TKey> where TKey : IEquatable<TKey>
    {
        public User<TKey>? User { get; set; }

        public Tenant<TKey> Tenant { get; set; }
    }
}
