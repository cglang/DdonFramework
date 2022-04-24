using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.KeyValueStorage
{
    public interface IDdonKeyValueManager<TValue, TOptions> where TOptions : DdonKvOptions
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
        /// <returns></returns>
        Task<TValue?> GetValueAsync(object key);

        /// <summary>
        /// 更改指定键的值 没有则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slice"></param>
        /// <returns></returns>
        Task<bool> SetValueAsync(object key, TValue value);

        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TValue?> GetValueAsync(string key);

        /// <summary>
        /// 更改指定键的值 没有则添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slice"></param>
        /// <returns></returns>
        Task<bool> SetValueAsync(string key, TValue value);

        /// <summary>
        /// 获取所有的键
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetAllKeyAsync();

        /// <summary>
        /// 获取所有的键
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TValue>> GetAllValueAsync();

        /// <summary>
        /// 获取所有的键/值
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, TValue>> GetAllKeyValueAsync();

        /// <summary>
        /// 删除指定键值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="slice"></param>
        Task<bool> DeleteValueAsync(string key);
    }
}
