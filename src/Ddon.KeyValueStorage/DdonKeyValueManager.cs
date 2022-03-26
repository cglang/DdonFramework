using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ddon.KeyValueStorage
{
    public class DdonKeyValueManager<TKey, TValue> : IDdonKeyValueManager<TKey, TValue> where TKey : struct
    {
        private readonly DdonDictionary<TKey, TValue?> _storage;

        /// <summary>
        /// 配置文件位置
        /// </summary>
        private readonly string _directoryPath;

        /// <summary>
        /// 自动保存
        /// </summary>
        private readonly bool _autoSave;

        private readonly string _slice = DdonKeyValueStorageConst.DefaultSlice;

        private DdonKeyValueManager(DdonKvOptions? option = default, string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            option ??= new DdonKvOptions();

            _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, option.Directory);
            _autoSave = option.AutoSave;
            _slice = slice;

            _storage = new(Path.Combine(_directoryPath, slice));
        }

        public DdonKeyValueManager(IOptions<DdonKvOptions> options, string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.Value.Directory);
            _autoSave = options.Value.AutoSave;
            _slice = slice;

            _storage = new(Path.Combine(_directoryPath, slice));
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns></returns>
        public static DdonKeyValueManager<TKey, TValue> CreateObject(DdonKvOptions? option = default, string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            return new DdonKeyValueManager<TKey, TValue>(option, slice);
        }

        public async Task<bool> SaveAsync(string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            return await _storage.SaveAsync();
        }

        public async Task<TValue?> GetValueAsync(TKey key)
        {
            try
            {
                await Task.CompletedTask;
                return _storage[key];
            }
            catch
            {
                return default;
            }
        }

        public async Task<IEnumerable<TValue?>> GetAllValueAsync()
        {
            try
            {
                await Task.CompletedTask;
                return _storage.Values.ToList();
            }
            catch
            {
                return new List<TValue?>();
            }
        }

        public async Task<IEnumerable<TKey>> GetAllKeyAsync()
        {
            try
            {
                await Task.CompletedTask;
                return _storage.Keys;
            }
            catch
            {
                return new List<TKey>();
            }
        }

        public async Task<Dictionary<TKey, TValue?>> GetAllKeyValueAsync()
        {
            await Task.CompletedTask;
            return _storage;
        }

        public async Task<bool> SetValueAsync(TKey key, TValue value)
        {
            try
            {
                _storage[key] = value;
                if (_autoSave) return await SaveAsync(_slice);
                return true;
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> DeleteValueAsync(TKey key)
        {
            try
            {
                _storage.Remove(key);
                if (_autoSave) return await SaveAsync(_slice);
                return true;
            }
            catch
            {
                return default;
            }
        }

    }
}
