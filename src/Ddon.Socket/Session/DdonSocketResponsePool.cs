using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STimer = System.Timers.Timer;

namespace Ddon.Socket.Session
{
    /// <summary>
    /// 响应集合
    /// </summary>
    internal class DdonSocketResponsePool
    {
        private static readonly object _lock = new();
        private static DdonSocketResponsePool? obj;

        public readonly Dictionary<Guid, DdonSocketResponseHandler> Pairs = new();

        private DdonSocketResponsePool()
        {
            //TODO: 超时请求处理
            //TODO: 超时移除应该用延时队列来做

            //STimer timer = new() { Enabled = true, Interval = 10000 };
            //timer.Elapsed += (_, _) =>
            //{
            //    var removeIds = Pairs.Values.Where(x => x.Time.AddSeconds(10) < DateTime.Now).Select(x => x.Id);
            //    Parallel.ForEach(removeIds, id =>
            //    {
            //        Pairs.Remove(id);
            //        Pairs[id].ExceptionThen?.Invoke(string.Empty);
            //    });
            //};
            //timer.Start();
        }

        public static DdonSocketResponsePool GetInstance()
        {
            if (obj != null) return obj;
            lock (_lock) obj ??= new DdonSocketResponsePool();
            return obj;
        }
    }
}
