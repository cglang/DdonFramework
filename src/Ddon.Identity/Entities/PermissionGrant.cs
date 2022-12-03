﻿using Ddon.Domain.Entities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Identity.Entities
{
    public class PermissionGrant<TKey> : Entity<TKey> where TKey : IEquatable<TKey>
    {
        [AllowNull]
        public string Name { get; set; }

        public PermissionGrantType Type { get; set; }

        [AllowNull]
        public TKey GrantKey { get; set; }
    }

    public enum PermissionGrantType
    {
        User = 1,
        Role = 2
    }
}
