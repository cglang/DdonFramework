using System;

namespace Ddon.Domain.Entities.Identity
{
    public class UserRole<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        public UserRole(TKey userId, TKey roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        public virtual TKey UserId { get; set; }

        public virtual TKey RoleId { get; set; }
    }
}
