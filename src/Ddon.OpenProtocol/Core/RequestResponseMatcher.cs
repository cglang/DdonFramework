using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Models;
using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter;
using OpenProtocolInterpreter.Communication;
using OpenProtocolInterpreter.Tightening;

namespace Ddon.OpenProtocol.Core
{
    public class RequestResponseMatcher
    {
        private readonly Func<int, int?> _getResponseMid;
        private readonly ILogger? _logger;

        private readonly ConcurrentDictionary<int, Queue<PendingRequest>> _queues = new();

        public RequestResponseMatcher(
            Func<int, int?> getResponseMid,
            ILogger? logger = null)
        {
            _getResponseMid = getResponseMid;
            _logger = logger;
        }

        public (Task<Mid> Task, int ExpectedMid) Enqueue(
            int requestMid,
            int timeoutMs,
            CancellationToken ct)
        {
            int? responseMid = _getResponseMid(requestMid);
            if (responseMid is null)
                throw new NotSupportedException(
                    $"No response mapping for MID {requestMid:D4}. " +
                    $"Use MapResponse or MapResponse<TReq, TRes> to register it.");

            var tcs = new TaskCompletionSource<Mid>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var queue = _queues.GetOrAdd(responseMid.Value, _ => new Queue<PendingRequest>());

            lock (queue)
            {
                queue.Enqueue(new PendingRequest(requestMid, tcs));
            }

            var timeoutCts = new CancellationTokenSource(timeoutMs);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, ct);

            linkedCts.Token.Register(() =>
            {
                TryDequeueSpecific(responseMid.Value, tcs);
                tcs.TrySetCanceled(linkedCts.Token);
                timeoutCts.Dispose();
                linkedCts.Dispose();
            }, useSynchronizationContext: false);

            return (tcs.Task, responseMid.Value);
        }

        public bool TryComplete(Mid response)
        {
            int responseMidNumber = response.Header.Mid;

            if (responseMidNumber == 4)
            {
                return TryHandleMid0004(response);
            }

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

        private bool TryHandleMid0004(Mid mid0004)
        {
            int failedMid = ExtractFailedCommandMid(mid0004);
            if (failedMid <= 0)
                return false;

            int? responseMid = _getResponseMid(failedMid);
            if (responseMid is null)
                return false;

            if (!_queues.TryGetValue(responseMid.Value, out var queue))
                return false;

            TaskCompletionSource<Mid>? tcs = null;

            lock (queue)
            {
                while (queue.Count > 0)
                {
                    var pending = queue.Dequeue();
                    if (!pending.IsExpired && pending.RequestMid == failedMid)
                    {
                        tcs = pending.Tcs;
                        break;
                    }
                }
            }

            if (tcs is null) return false;

            string errorCode = ExtractErrorCode(mid0004);
            tcs.TrySetException(
                new OpenProtocolException(failedMid, errorCode));
            return true;
        }

        private static int ExtractFailedCommandMid(Mid mid0004)
        {
            try
            {
                var mid = mid0004;
                var field = mid.GetType().GetProperty("FailedCommandMid");
                if (field is not null)
                {
                    var val = field.GetValue(mid);
                    if (val is int i) return i;
                }

                var midField = mid.GetType().GetField("FailedCommandMid");
                if (midField is not null)
                {
                    var val = midField.GetValue(mid);
                    if (val is int i) return i;
                }
            }
            catch { }

            return 0;
        }

        private static string ExtractErrorCode(Mid mid0004)
        {
            try
            {
                var field = mid0004.GetType().GetProperty("ErrorCode");
                if (field is not null)
                {
                    var val = field.GetValue(mid0004);
                    if (val is string s) return s;
                }

                var fld = mid0004.GetType().GetField("ErrorCode");
                if (fld is not null)
                {
                    var val = fld.GetValue(mid0004);
                    if (val is string s) return s;
                }
            }
            catch { }

            return "0000";
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

        private sealed class PendingRequest
        {
            public int RequestMid { get; }
            public TaskCompletionSource<Mid> Tcs { get; }
            public bool IsExpired => Tcs.Task.IsCompleted;

            public PendingRequest(int requestMid, TaskCompletionSource<Mid> tcs)
            {
                RequestMid = requestMid;
                Tcs = tcs;
            }
        }
    }
}
