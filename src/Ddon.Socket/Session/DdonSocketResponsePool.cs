using Ddon.Core.Use.Queue;
using System;
using System.Collections.Generic;
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
            DelayQueue.Add(new(TimeSpan.FromSeconds(100), ddonSocketResponseHandler));
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

                while (DelayQueue.IsEmpty)
                {
                    var task = DelayQueue.Take();
                    if (task != null)
                    {
                        if (!task.Item.IsCompleted)
                            task.Item.ExceptionThen.Invoke("请求超时");
                    }
                }

                state = false;
            });
        }
    }
}
