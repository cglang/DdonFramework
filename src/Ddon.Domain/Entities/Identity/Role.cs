using Ddon.Domain.Entities;
using System;

namespace Ddon.Identity.Entities
{
    public class Role<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 规范化名称
        /// </summary>
        public string? NormalizedName { get; set; }

        /// <summary>
        /// 并发戳
        /// </summary>
        public string? ConcurrencyStamp { get; set; }
    }
}
