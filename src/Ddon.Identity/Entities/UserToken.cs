using System;
using Microsoft.AspNetCore.Identity;

namespace Ddon.Identity.Entities
{
    public class UserToken<TKey> : IdentityUserToken<TKey>
        where TKey : IEquatable<TKey>
    {
    }
}
