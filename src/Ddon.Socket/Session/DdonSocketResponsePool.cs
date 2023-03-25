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
    internal class DdonSocketResponsePool
    {
        internal static void Add(DdonSocketResponseHandler ddonSocketResponseHandler)
        {
            Datas.Instance.Pairs.Add(ddonSocketResponseHandler.Id, ddonSocketResponseHandler);
            Datas.Instance.DelayQueue.Add(ddonSocketResponseHandler.Id, TimeSpan.FromSeconds(10));
        }

        internal static bool ContainsKey(Guid id) => Datas.Instance.Pairs.ContainsKey(id);

        internal static DdonSocketResponseHandler Get(Guid id) => Datas.Instance.Pairs[id];

        internal static void Remove(Guid id) => Datas.Instance.Pairs.Remove(id);

        internal static void Start() => Datas.Instance.Start();

        internal static void Dispose() => Datas.Instance.Dispose();

        internal class Datas : IDisposable
        {
            private Datas()
            {
                DelayQueue = new();
                Pairs = new();
            }

            public static Datas Instance = new Lazy<Datas>(() => new Datas()).Value;

            public readonly Dictionary<Guid, DdonSocketResponseHandler> Pairs;
            public readonly DelayQueue<Guid> DelayQueue;

            internal void Start()
            {
                Task.Run(async () =>
                {
                    try
                    {
                        while (_disposed == false)
                        {
                            while (!Instance.DelayQueue.IsEmpty)
                            {
                                var item = await Instance.DelayQueue.TakeAsync();
                                if (item != default && Pairs.ContainsKey(item) && !Pairs[item].IsCompleted)
                                {
                                    Pairs[item].ExceptionThen.Invoke("请求超时");
                                }
                            }
                            Thread.Sleep(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.Source);
                        Console.WriteLine(ex.HelpLink);
                    }
                });
            }

            private bool _disposed;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                }

                DelayQueue.Clear();

                _disposed = true;
            }
        }
    }
}
