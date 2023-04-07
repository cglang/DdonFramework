using System;
using Microsoft.AspNetCore.Identity;

namespace Ddon.Identity.Entities
{
    public class UserLogin<TKey> : IdentityUserLogin<TKey>
        where TKey : IEquatable<TKey>
    {
    }
}
