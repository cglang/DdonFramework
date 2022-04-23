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
    public class DdonSocketResponsePool
    {
        private static readonly object _lock = new();
        private static DdonSocketResponsePool? obj;

        public readonly Dictionary<Guid, DdonSocketResponseBody> Pairs = new();

        private DdonSocketResponsePool()
        {
            // 检测剪切板变化
            STimer timer = new() { Enabled = true, Interval = 10000 };
            timer.Elapsed += (_, _) =>
            {
                lock (_lock)
                {
                    var removeIds = Pairs.Values.Where(x => x.Time.AddMinutes(5) < DateTime.Now).Select(x => x.Id);
                    Parallel.ForEach(removeIds, id =>
                    {
                        Pairs[id].ExceptionThen?.Invoke(default);
                        Pairs.Remove(id);
                    });
                    //Pairs.RemoveAll(x => x.Value.Time.AddSeconds(5) < DateTime.Now);
                }
            };
            timer.Start();
        }

        public static DdonSocketResponsePool GetInstance()
        {
            if (obj != null) return obj;
            lock (_lock) obj ??= new DdonSocketResponsePool();
            return obj;
        }

        public void Add(Guid id, DdonSocketResponseBody response)
        {
            Pairs.Add(id, response);
        }
    }
}
