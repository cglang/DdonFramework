using System.Collections.Generic;

namespace Ddon.KeyValueStorage
{
    public class DdonKvStorageFactory<TKey, TValue> where TKey : struct
    {
        private static readonly Dictionary<string, IDdonKeyValueManager<TKey, TValue>> _manager = new();

        private static readonly object _lock = new();

        public static IDdonKeyValueManager<TKey, TValue> GetInstance(DdonKvOptions? option = default, string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            if (!_manager.ContainsKey(slice))
            {
                lock (_lock)
                {
                    if (!_manager.ContainsKey(slice))
                    {
                        _manager[slice] = DdonKeyValueManager<TKey, TValue>.CreateObject(option, slice);
                    }
                }
            }
            return _manager[slice];
        }
    }
}
