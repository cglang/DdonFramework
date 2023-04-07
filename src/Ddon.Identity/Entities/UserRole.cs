using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Ddon.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Ddon.Identity.Entities
{
    public class UserRole<TKey> : IdentityUserRole<TKey>, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        [Key, NotNull, Column(Order = 0)]
        public TKey Id { get; set; } = default!;
    }
}
