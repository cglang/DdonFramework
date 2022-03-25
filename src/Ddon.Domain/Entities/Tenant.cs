using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Domain.Entities
{
    public class Tenant<TKey> : Entity<TKey>
        where TKey : IEquatable<TKey>
    {
        [AllowNull]
        public string Name { get; set; } = "Default";
    }
}
