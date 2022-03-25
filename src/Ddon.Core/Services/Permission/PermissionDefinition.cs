using System.Collections.Generic;
using System.Linq;

namespace Ddon.Core.Services.Permission
{
    public class PermissionDefinition
    {
        public string Name { get; }

        public string DisplayName { get; }

        public IReadOnlyList<PermissionDefinition> Children => _children.ToList();

        private readonly List<PermissionDefinition> _children;

        public Dictionary<string, object> Properties { get; }

        [System.Text.Json.Serialization.JsonIgnore]
        public PermissionDefinition? Parent { get; private set; }

        public virtual PermissionDefinition AddChild(string name, string displayName)
        {
            var child = new PermissionDefinition(name, displayName) { Parent = this };

            _children.Add(child);

            return child;
        }

        protected internal PermissionDefinition(string name, string displayName)
        {
            Name = Check.NotNull(name, nameof(name));
            DisplayName = Check.NotNull(displayName, nameof(displayName));

            Properties = new Dictionary<string, object>();

            _children = new List<PermissionDefinition>();
        }
    }
}
