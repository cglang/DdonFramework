using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.KeyValueStorage
{
    public interface IDdonKeyValueManager<TKey, TValue> where TKey : notnull
    {
        /// <summary>
        /// 保存修改
        /// </summary>
        /// <param name="slice">片</param>
        /// <returns></returns>
        Task<bool> SaveAsync(string slice = DdonKeyValueStorageConst.DefaultSlice);

        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slice"></param>
        /// <returns></returns>
        Task<TValue?> GetValueAsync(TKey key);

        /// <summary>
        /// 更改指定键的值 没有则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slice"></param>
        /// <returns></returns>
        Task<bool> SetValueAsync(TKey key, TValue value);

        /// <summary>
        /// 获取所有的键
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TKey>> GetAllKeyAsync();

        /// <summary>
        /// 获取所有的键
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TValue?>> GetAllValueAsync();

        /// <summary>
        /// 获取所有的键/值
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<TKey, TValue?>> GetAllKeyValueAsync();

        /// <summary>
        /// 删除指定键值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slice"></param>
        Task<bool> DeleteValueAsync(TKey key);
    }


    public interface IDdonKeyValueStorages<TValue> : IDdonKeyValueManager<string, TValue>
    {
        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slice"></param>
        /// <returns></returns>
        new Task<TValue?> GetValueAsync(string key);

        /// <summary>
        /// 更改指定键的值 没有则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slice"></param>
        /// <returns></returns>
        new Task<bool> SetValueAsync(string key, TValue value);

        /// <summary>
        /// 获取所有的键
        /// </summary>
        /// <returns></returns>
        new Task<IEnumerable<string?>> GetAllKeyAsync();

        /// <summary>
        /// 删除指定键值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slice"></param>
        new Task<bool> DeleteValueAsync(string key);
    }
}
