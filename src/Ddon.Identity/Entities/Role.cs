using Ddon.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ddon.Identity.Entities
{
    public class Role<TKey> : IdentityRole<TKey>, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 角色权限
        /// </summary>
        [NotMapped]
        public List<PermissionGrant<TKey>>? RolePermissions { get; set; }
    }
}
