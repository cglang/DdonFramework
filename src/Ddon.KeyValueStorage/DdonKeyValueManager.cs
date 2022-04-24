using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ddon.KeyValueStorage
{
    public class DdonKeyValueManager<TValue, TOptions> : IDdonKeyValueManager<TValue, TOptions> where TOptions : DdonKvOptions
    {
        private readonly DdonDictionary<TValue> _storage;

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

        public DdonKeyValueManager(IOptions<TOptions> options, string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, options.Value.Directory);
            _autoSave = options.Value.AutoSave;
            _slice = slice;

            _storage = new DdonDictionary<TValue>(Path.Combine(_directoryPath, slice));
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns></returns>
        public static DdonKeyValueManager<TValue, DdonKvOptions> CreateObject(DdonKvOptions? option = default, string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            return new DdonKeyValueManager<TValue, DdonKvOptions>(option, slice);
        }

        public async Task<bool> SaveAsync(string slice = DdonKeyValueStorageConst.DefaultSlice)
        {
            return await _storage.SaveAsync();
        }

        public async Task<TValue?> GetValueAsync(object key)
        {
            return await GetValueAsync(key.ToString() ?? throw new Exception("不允许此类型的Key"));
        }

        public async Task<TValue?> GetValueAsync(string key)
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

        public async Task<IEnumerable<TValue>> GetAllValueAsync()
        {
            try
            {
                await Task.CompletedTask;
                return _storage.Values.ToList();
            }
            catch
            {
                return new List<TValue>();
            }
        }

        public async Task<IEnumerable<string>> GetAllKeyAsync()
        {
            try
            {
                await Task.CompletedTask;
                return _storage.Keys;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, TValue>> GetAllKeyValueAsync()
        {
            await Task.CompletedTask;
            return _storage;
        }

        public async Task<bool> SetValueAsync(object key, TValue value)
        {
            return await SetValueAsync(key.ToString() ?? throw new Exception("不允许此类型的Key"), value);
        }

        public async Task<bool> SetValueAsync(string key, TValue value)
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

        public async Task<bool> DeleteValueAsync(string key)
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
