using Ddon.Core.DependencyInjection;

namespace Ddon.Core.Services.Permission
{
    public interface IPermissionDefinitionContext : ISingletonDependency
    {
        PermissionGroupDefinition GetGroup(string name);

        PermissionGroupDefinition? GetGroupOrNull(string name);

        PermissionGroupDefinition AddGroup(string name, string displayName);

        void RemoveGroup(string name);

        PermissionDefinition? GetPermissionOrNull(string name);
    }
}
