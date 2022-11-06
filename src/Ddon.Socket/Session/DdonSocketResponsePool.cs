using Ddon.Core.Use.Queue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    /// <summary>
    /// 响应集合
    /// </summary>
    internal class DdonSocketResponsePool : IDisposable
    {
        private DdonSocketResponsePool()
        {
            DelayQueue = new();
            Pairs = new();
        }

        public static DdonSocketResponsePool Instance = new Lazy<DdonSocketResponsePool>(() => new DdonSocketResponsePool()).Value;

        public readonly Dictionary<Guid, DdonSocketResponseHandler> Pairs;
        public readonly DelayQueue<Guid> DelayQueue;

        internal static void Add(DdonSocketResponseHandler ddonSocketResponseHandler)
        {
            Instance.Pairs.Add(ddonSocketResponseHandler.Id, ddonSocketResponseHandler);
            Instance.DelayQueue.AddAsync(ddonSocketResponseHandler.Id, TimeSpan.FromSeconds(1)).Wait();
        }

        internal bool ContainsKey(Guid id)
        {
            return Pairs.ContainsKey(id);
        }

        internal DdonSocketResponseHandler Get(Guid id)
        {
            return Pairs[id];
        }

        internal void Remove(Guid id)
        {
            Pairs.Remove(id);
        }

        internal void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        while (!Instance.DelayQueue.IsEmpty)
                        {
                            var item = await Instance.DelayQueue.TakeAsync();
                            if (item != default)
                            {
                                var handle = Instance.Get(item);
                                if (!handle.IsCompleted)
                                    handle.ExceptionThen.Invoke("请求超时");
                            }
                        }
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("出错了");
                }
            });
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
