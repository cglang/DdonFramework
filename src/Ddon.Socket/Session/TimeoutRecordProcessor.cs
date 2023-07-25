﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ddon.Core.Use.Queue;

namespace Ddon.Socket.Session
{
    /// <summary>
    /// 超时记录处理
    /// </summary>
    internal class TimeoutRecordProcessor
    {
        internal static void Add(RequestEventListener ddonSocketResponseHandler)
        {
            Datas.Instance.Pairs.Add(ddonSocketResponseHandler.Id, ddonSocketResponseHandler);
            Datas.Instance.DelayQueue.Add(ddonSocketResponseHandler.Id, TimeSpan.FromSeconds(100));
        }

        internal static bool ContainsKey(Guid id) => Datas.Instance.Pairs.ContainsKey(id);

        internal static RequestEventListener Get(Guid id) => Datas.Instance.Pairs[id];

        internal static void Remove(Guid id) => Datas.Instance.Pairs.Remove(id);

        internal static void Start() => Datas.Instance.Start();

        internal class Datas
        {
            private Datas()
            {
                DelayQueue = new();
                Pairs = new();
            }

            public static Datas Instance = new Lazy<Datas>(() => new Datas()).Value;

            public readonly Dictionary<Guid, RequestEventListener> Pairs;
            public readonly DelayQueue<Guid> DelayQueue;

            internal void Start() => Task.Run(PollTimeoutRequests);

            async Task PollTimeoutRequests()
            {
                while (true)
                {
                    try
                    {
                        var item = await Instance.DelayQueue.TakeAsync();
                        if (item != default && Pairs.TryGetValue(item, out var value) && !value.IsCompleted)
                        {
                            Pairs[item].ExceptionThen.Invoke("请求超时");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.Source);
                        Console.WriteLine(ex.HelpLink);
                    }
                }
            }
        }
    }
}
