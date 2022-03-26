﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.KeyValueStorage
{
    public class DdonLocks
    {
        public static readonly HashSet<string> Locks = new();
    }

    public class DdonLock : IDisposable
    {
        private string _key = string.Empty;

        /// <summary>
        /// 获取一个锁
        /// </summary>
        /// <param name="key">锁key</param>
        /// <param name="frequency">尝试次数</param>
        /// <param name="interval">间隔时间(ms)</param>
        /// <returns></returns>
        public async Task<bool> GetLock(string key, int frequency = 10, int interval = 100)
        {
            _key = key;
            int i = 0;
            do
            {
                if (!DdonLocks.Locks.Contains(key))
                {
                    lock (DdonLocks.Locks)
                    {
                        DdonLocks.Locks.Add(key);
                        return true;
                    }
                }
                await Task.Delay(interval);
                i++;
            } while (frequency > i);
            return false;
        }

        public void Dispose()
        {
            DdonLocks.Locks.Remove(_key);
            GC.SuppressFinalize(this);
        }
    }
}
