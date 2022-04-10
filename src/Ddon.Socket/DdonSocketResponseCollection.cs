using Ddon.ConvenientSocket.Extra;
using STimer = System.Timers.Timer;

namespace Ddon.Socket
{
    /// <summary>
    /// 响应集合
    /// </summary>
    public class DdonSocketResponseCollection
    {
        private static readonly object _lock = new();
        private static DdonSocketResponseCollection? obj;

        public readonly Dictionary<Guid, DdonSocketResponse> Pairs = new();

        private DdonSocketResponseCollection()
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

        public static DdonSocketResponseCollection GetInstance()
        {
            if (obj != null) return obj;
            lock (_lock) obj ??= new DdonSocketResponseCollection();
            return obj;
        }

        public void Add(Guid id, DdonSocketResponse response)
        {
            Pairs.Add(id, response);
        }

        /// <summary>
        /// 响应处理
        /// </summary>
        /// <param name="id"></param>
        /// <param name="info"></param>
        public void ResponseHandle(DdonSocketPackageInfo<string> info)
        {
            var id = info.Head.Id;
            if (Pairs.ContainsKey(id))
            {
                lock (Pairs)
                {
                    if (info.Head.Code == ResponseCode.OK)
                        Pairs[id].ActionThen?.Invoke(info);
                    else if (info.Head.Code == ResponseCode.Error)
                        Pairs[id].ExceptionThen?.Invoke(info);
                }
            }
        }
    }

    public class DdonSocketResponse
    {
        public Guid Id { get; set; }

        public DateTime Time { get; set; }

        public Action<DdonSocketPackageInfo<string>>? ActionThen;
        public Action<DdonSocketPackageInfo<string>>? ExceptionThen;

        public DdonSocketResponse(Guid id)
        {
            Id = id;
            Time = DateTime.Now;
        }

        public DdonSocketResponse Then(Action<DdonSocketPackageInfo<string>> action)
        {
            ActionThen = action;
            DdonSocketResponseCollection.GetInstance().Pairs.Add(Id, this);
            return this;
        }

        public DdonSocketResponse Exception(Action<DdonSocketPackageInfo<string>> action)
        {
            ExceptionThen = action;
            return this;
        }
    }
}
