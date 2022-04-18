using Ddon.Core;
using System;
using System.Collections.Generic;

namespace Ddon.Identity.Permission
{
    public class PermissionDefinitionContext : IPermissionDefinitionContext
    {
        public Dictionary<string, PermissionGroupDefinition> Groups { get; }

        public PermissionDefinitionContext(IPermissionDefinitionProvider permissionDefinitionProvider)
        {
            Groups ??= new Dictionary<string, PermissionGroupDefinition>();
            permissionDefinitionProvider.Define(this);
        }

        public virtual PermissionGroupDefinition AddGroup(string name, string displayName)
        {
            Check.NotNull(name, nameof(name));

            if (Groups.ContainsKey(name))
            {
                throw new Exception($"There is already an existing permission group with name: {name}");
            }

            return Groups[name] = new PermissionGroupDefinition(name, displayName);
        }

        public virtual PermissionGroupDefinition GetGroup(string name)
        {
            var group = GetGroupOrNull(name);

            if (group == null)
            {
                throw new Exception($"Could not find a permission definition group with the given name: {name}");
            }

            return group;
        }

        public virtual PermissionGroupDefinition? GetGroupOrNull(string name)
        {
            Check.NotNull(name, nameof(name));

            if (!Groups.ContainsKey(name))
            {
                return null;
            }

            return Groups[name];
        }

        public virtual void RemoveGroup(string name)
        {
            Check.NotNull(name, nameof(name));

            if (!Groups.ContainsKey(name))
            {
                throw new Exception($"Not found permission group with name: {name}");
            }

            Groups.Remove(name);
        }

        public virtual PermissionDefinition? GetPermissionOrNull(string name)
        {
            Check.NotNull(name, nameof(name));

            foreach (var groupDefinition in Groups.Values)
            {
                var permissionDefinition = groupDefinition.GetPermissionOrNull(name);

                if (permissionDefinition != null)
                {
                    return permissionDefinition;
                }
            }

            return null;
        }
    }
}
