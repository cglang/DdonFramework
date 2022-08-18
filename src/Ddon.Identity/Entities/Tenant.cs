using System;

namespace Ddon.Domain.Entities
{
    public class Tenant<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        public string Name { get; set; } = "Default";
    }
}
