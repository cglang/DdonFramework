using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Ddon.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Ddon.Identity.Entities
{
    public class User<TKey> : IdentityUser<TKey>, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 用户角色
        /// </summary>
        [NotMapped]
        public List<Role<TKey>>? UserRoles { get; set; }

        /// <summary>
        /// 用户权限
        /// </summary>
        [NotMapped]
        public List<PermissionGrant<TKey>>? UserPermissions { get; set; }
    }
}
