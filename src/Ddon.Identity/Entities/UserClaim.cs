using System;
using Microsoft.AspNetCore.Identity;

namespace Ddon.Identity.Entities
{
    public class UserClaim<TKey> : IdentityUserClaim<TKey>
        where TKey : IEquatable<TKey>
    {
    }
}
