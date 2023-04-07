using Ddon.Identity.Entities;
using System;

namespace Ddon.Domain.UserInfo
{
    public interface ICurrentUserInfoAccessor<TKey> where TKey : IEquatable<TKey>
    {
        User<TKey>? User { get; set; }
    }
}
