using System;
using Ddon.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Ddon.Identity.Entities
{
    public class RoleClaim<TKey> : IdentityRoleClaim<TKey>
        where TKey : IEquatable<TKey>
    {
    }
}
