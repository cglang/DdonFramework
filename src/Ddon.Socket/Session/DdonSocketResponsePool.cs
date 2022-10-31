using Ddon.Core.Use.DelayQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    /// <summary>
    /// 响应集合
    /// </summary>
    internal static class DdonSocketResponsePool
    {
        private static readonly Dictionary<Guid, DdonSocketResponseHandler> Pairs = new();

        private static readonly DelayQueue<DelayItem<DdonSocketResponseHandler>> DelayQueue = new();

        internal static void Add(DdonSocketResponseHandler ddonSocketResponseHandler)
        {
            Pairs.Add(ddonSocketResponseHandler.Id, ddonSocketResponseHandler);
            DelayQueue.TryAdd(new(TimeSpan.FromSeconds(1), ddonSocketResponseHandler));
            Start();
        }

        internal static bool ContainsKey(Guid id)
        {
            return Pairs.ContainsKey(id);
        }

        internal static DdonSocketResponseHandler Get(Guid id)
        {
            return Pairs[id];
        }

        internal static void Remove(Guid id)
        {
            Pairs.Remove(id);
        }

        private static bool state = false;
        private static void Start()
        {
            if (state) return;
            Task.Run(() =>
            {
                state = true;

                while (DelayQueue.Count > 0)
                {
                    if (DelayQueue.TryTake(out var task))
                    {
                        Console.WriteLine("啦啦啦");
                        if (!task.Item.IsCompleted)
                            task.Item.ExceptionThen.Invoke("请求超时");
                    }
                }

                state = false;
            });
        }
    }
}
