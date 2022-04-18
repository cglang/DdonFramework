using Ddon.Core;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Ddon.Identity.Permission
{
    public class PermissionGroupDefinition
    {
        public string Name { get; }

        public string DisplayName { get; }


        public Dictionary<string, object> Properties { get; }

        public IReadOnlyList<PermissionDefinition> Permissions => _permissions.ToImmutableList();

        private readonly List<PermissionDefinition> _permissions;

        protected internal PermissionGroupDefinition(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;

            Properties = new Dictionary<string, object>();

            _permissions = new List<PermissionDefinition>();
        }

        public virtual PermissionDefinition AddPermission(string name, string displayName)
        {
            var permission = new PermissionDefinition(name, displayName);

            _permissions.Add(permission);

            return permission;
        }

        public PermissionDefinition? GetPermissionOrNull(string name)
        {
            Check.NotNull(name, nameof(name));

            return GetPermissionOrNullRecursively(Permissions, name);
        }

        private PermissionDefinition? GetPermissionOrNullRecursively(IReadOnlyList<PermissionDefinition> permissions, string name)
        {
            foreach (var permission in permissions)
            {
                if (permission.Name == name)
                {
                    return permission;
                }

                var childPermission = GetPermissionOrNullRecursively(permission.Children, name);
                if (childPermission != null)
                {
                    return childPermission;
                }
            }

            return null;
        }
    }
}
