using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using NLua;

namespace Ddon.LuaEngine
{
    internal class LuaVmManager : ILuaVmManager
    {
        private readonly ConcurrentDictionary<string, Lua> _vms = new ConcurrentDictionary<string, Lua>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, Lua> GetAllVms()
        {
            return new Dictionary<string, Lua>(_vms);
        }

        public Lua GetVm(string groupName)
        {
            _vms.TryGetValue(groupName, out var vm);
            return vm;
        }

        public bool ContainsVm(string groupName)
        {
            return _vms.ContainsKey(groupName);
        }

        public Lua AddVm(string groupName)
        {
            var vm = new Lua();
            vm.State.Encoding = Encoding.UTF8;
            if (!_vms.TryAdd(groupName, vm))
            {
                vm.Dispose();
                throw new InvalidOperationException($"Lua VM for group '{groupName}' already exists.");
            }

            return vm;
        }

        public void AddVm(string groupName, Lua vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            if (!_vms.TryAdd(groupName, vm))
                throw new InvalidOperationException($"Lua VM for group '{groupName}' already exists.");
        }

        public bool RemoveVm(string groupName)
        {
            if (_vms.TryRemove(groupName, out var vm))
            {
                vm.Dispose();
                return true;
            }

            return false;
        }

        public void SetVm(string groupName, Lua vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            if (_vms.TryRemove(groupName, out var oldVm))
            {
                oldVm.Dispose();
            }

            if (!_vms.TryAdd(groupName, vm))
                throw new InvalidOperationException($"Failed to set Lua VM for group '{groupName}'.");
        }

        public void ClearAllVms()
        {
            foreach (var kvp in _vms)
            {
                kvp.Value.Dispose();
            }

            _vms.Clear();
        }
    }
}
