using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ddon.ConvenientSocket.Exceptions;
using Ddon.Core.Use.Queue;

namespace Ddon.Socket.Session
{
    /// <summary>
    /// 超时记录处理
    /// </summary>
    internal static class TimeoutRecordProcessor
    {
        private static Dictionary<Guid, RequestEventListener> _pairs = new();
        private static DelayQueue<Guid> _delayQueue = new();

        internal static void Add(RequestEventListener ddonSocketResponseHandler)
        {
            _pairs.Add(ddonSocketResponseHandler.Id, ddonSocketResponseHandler);
            _delayQueue.Add(ddonSocketResponseHandler.Id, TimeSpan.FromSeconds(60));
        }

        internal static void Remove(Guid id)
        {
            _pairs.Remove(id);
            _delayQueue.Remove(id);
        }

        internal static bool ContainsKey(Guid id)
        {
            return _pairs.ContainsKey(id);
        }

        internal static RequestEventListener? GetDefault(Guid id)
        {
            return _pairs.ContainsKey(id) ? _pairs[id] : null;
        }

        internal static void Start()
        {
            Task.Run(PollTimeoutRequests);
        }

        private static async Task PollTimeoutRequests()
        {
            while (true)
            {
                var item = await _delayQueue.TakeAsync();
                if (item != default && _pairs.TryGetValue(item, out var listener) && !listener.IsCompleted)
                {
                    _pairs[item].ExceptionHandler(new DdonSocketRequestException("请求超时"));
                }
            }
        }
    }
}
