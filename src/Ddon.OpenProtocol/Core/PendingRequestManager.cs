using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.Tightening;
using OpenProtocolInterpreter.Alarm;
using OpenProtocolInterpreter.ParameterSet;
using OpenProtocolInterpreter.Tool;
using OpenProtocolInterpreter.IOInterface;

namespace Ddon.OpenProtocol.Core
{
    public sealed class PendingRequestManager
    {
        private static readonly Dictionary<int, int> DefaultResponseMap = new()
        {
            [Mid0001.MID] = Mid0002.MID,
            [Mid0003.MID] = Mid0005.MID,
            [Mid0018.MID] = Mid0005.MID,
            [Mid0042.MID] = Mid0005.MID,
            [Mid0043.MID] = Mid0005.MID,
            [Mid0060.MID] = Mid0061.MID,
            [Mid0062.MID] = Mid0005.MID,
            [Mid0071.MID] = Mid0072.MID,
            [Mid0074.MID] = Mid0005.MID,
            [Mid0224.MID] = Mid0005.MID,
        };

        private readonly ConcurrentDictionary<int, int> _responseMap;
        private readonly ConcurrentDictionary<int, Queue<PendingRequest>> _queues = new();

        public PendingRequestManager()
            : this(new Dictionary<int, int>())
        {
        }

        public PendingRequestManager(IDictionary<int, int> additionalMappings)
        {
            _responseMap = new ConcurrentDictionary<int, int>(DefaultResponseMap);
            foreach (var kvp in additionalMappings)
                _responseMap[kvp.Key] = kvp.Value;
        }

        public void AddMapping(int requestMid, int responseMid)
        {
            _responseMap[requestMid] = responseMid;
        }

        public HashSet<int> GetAllKnownMids()
        {
            var mids = new HashSet<int>();
            foreach (var kvp in _responseMap)
            {
                mids.Add(kvp.Key);
                mids.Add(kvp.Value);
            }
            return mids;
        }

        public (Task<Mid> Task, int ExpectedMid) Enqueue(
            int requestMidNumber,
            int timeoutMs,
            CancellationToken ct)
        {
            if (!_responseMap.TryGetValue(requestMidNumber, out int responseMid))
                throw new NotSupportedException(
                    $"No response mapping for MID {requestMidNumber:D4}. " +
                    $"Add a mapping via MapResponse or PendingRequestManager.");

            var tcs = new TaskCompletionSource<Mid>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var queue = _queues.GetOrAdd(responseMid, _ => new Queue<PendingRequest>());

            lock (queue)
            {
                queue.Enqueue(new PendingRequest(tcs));
            }

            var timeoutCts = new CancellationTokenSource(timeoutMs);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, ct);

            linkedCts.Token.Register(() =>
            {
                TryDequeueSpecific(responseMid, tcs);
                tcs.TrySetCanceled(linkedCts.Token);
                timeoutCts.Dispose();
                linkedCts.Dispose();
            }, useSynchronizationContext: false);

            return (tcs.Task, responseMid);
        }

        public bool TryComplete(int responseMidNumber, Mid response)
        {
            if (!_queues.TryGetValue(responseMidNumber, out var queue))
                return false;

            TaskCompletionSource<Mid>? tcs = null;

            lock (queue)
            {
                while (queue.Count > 0)
                {
                    var pending = queue.Dequeue();
                    if (!pending.IsExpired)
                    {
                        tcs = pending.Tcs;
                        break;
                    }
                }
            }

            if (tcs is null) return false;

            tcs.TrySetResult(response);
            return true;
        }

        public void FailAll(Exception reason)
        {
            foreach (var kvp in _queues)
            {
                lock (kvp.Value)
                {
                    while (kvp.Value.Count > 0)
                        kvp.Value.Dequeue().Tcs.TrySetException(reason);
                }
            }
        }

        private void TryDequeueSpecific(int responseMid, TaskCompletionSource<Mid> target)
        {
            if (!_queues.TryGetValue(responseMid, out var queue)) return;

            lock (queue)
            {
                var snapshot = queue.ToArray();
                queue.Clear();
                foreach (var item in snapshot)
                {
                    if (ReferenceEquals(item.Tcs, target)) continue;
                    queue.Enqueue(item);
                }
            }
        }

        private sealed class PendingRequest(TaskCompletionSource<Mid> tcs)
        {
            public TaskCompletionSource<Mid> Tcs { get; } = tcs;
            public bool IsExpired => Tcs.Task.IsCompleted;
        }
    }
}
