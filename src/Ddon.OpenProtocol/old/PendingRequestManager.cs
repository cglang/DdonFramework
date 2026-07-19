using System.Collections.Concurrent;
using OpenProtocolInterpreter;                  // NuGet: Mid base class
using OpenProtocolInterpreter.Communication;    // Mid0001, Mid0002, Mid0003, Mid0005
using OpenProtocolInterpreter.Tightening;       // Mid0060, Mid0061, Mid0062
using OpenProtocolInterpreter.Alarm;            // Mid0071, Mid0072, Mid0074
using OpenProtocolInterpreter.ParameterSet;     // Mid0018
using OpenProtocolInterpreter.Tool;
using OpenProtocolInterpreter.IOInterface;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
//using OpenProtocolInterpreter.IOInterface;             // Mid0042, Mid0043

namespace OpenProtocol.Pending;

/// <summary>
/// Request → Response MID 号映射 + FIFO 队列匹配。
///
/// Open Protocol 没有 Correlation ID。
/// 同一 Response MID 的并发请求用 FIFO 队列：先发先完成，不会串线。
///
/// 扩展方式：在 ResponseMap 中添加新的 Request→Response 映射即可。
/// </summary>
public sealed class PendingRequestManager
{
    // Request MID → Response MID
    // 覆盖 Open Protocol 标准规范中定义的常用请求响应对
    private static readonly Dictionary<int, int> ResponseMap = new()
    {
        // ── Communication ──────────────────────────────────────────
        [Mid0001.MID] = Mid0002.MID,   // 1  → 2    通讯开始
        [Mid0003.MID] = Mid0005.MID,   // 3  → 5    通讯结束（控制器回 Mid0005 ACK）

        // ── 通用 ACK（Mid0005）─────────────────────────────────────
        // 大多数"指令类"MID 控制器均回 Mid0005 表示接受
        [Mid0018.MID] = Mid0005.MID,   // 18 → 5    选择参数组
        [Mid0042.MID] = Mid0005.MID,   // 42 → 5    禁用工具
        [Mid0043.MID] = Mid0005.MID,   // 43 → 5    使能工具

        // ── Tightening ─────────────────────────────────────────────
        [Mid0060.MID] = Mid0061.MID,   // 60 → 61   订阅拧紧结果（首次响应）
        [Mid0062.MID] = Mid0005.MID,   // 62 → 5    取消订阅拧紧结果

        // ── Alarm ──────────────────────────────────────────────────
        [Mid0071.MID] = Mid0072.MID,   // 71 → 72   订阅报警（首次响应）
        [Mid0074.MID] = Mid0005.MID,   // 74 → 5    取消订阅报警

        [Mid0224.MID] = Mid0005.MID,   // 
    };

    // ResponseMid → FIFO Queue<PendingRequest>
    private readonly ConcurrentDictionary<int, Queue<PendingRequest>> _queues = new();

    // ─── Enqueue ──────────────────────────────────────────────────

    /// <summary>
    /// 注册一个等待响应的请求。
    /// </summary>
    /// <param name="requestMidNumber">发出的 MID 号（Mid.MidNumber 或 MidXXXX.MID）</param>
    /// <param name="timeoutMs">超时毫秒，超时后 Task 抛 OperationCanceledException</param>
    /// <param name="ct">外部取消令牌</param>
    /// <returns>等待响应的 Task 和期望的 Response MID 号</returns>
    public (Task<Mid> Task, int ExpectedMid) Enqueue(
        int requestMidNumber,
        int timeoutMs,
        CancellationToken ct)
    {
        if (!ResponseMap.TryGetValue(requestMidNumber, out int responseMid))
            throw new NotSupportedException(
                $"No response mapping for MID {requestMidNumber:D4}. " +
                $"Add it to PendingRequestManager.ResponseMap.");

        var tcs = new TaskCompletionSource<Mid>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var queue = _queues.GetOrAdd(responseMid, _ => new Queue<PendingRequest>());

        lock (queue)
        {
            queue.Enqueue(new PendingRequest(tcs));
        }

        // 超时 + 外部取消合并
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

    // ─── TryComplete ──────────────────────────────────────────────

    /// <summary>
    /// 收到响应时调用，完成最早等待的匹配请求。
    /// </summary>
    /// <returns>是否匹配到等待请求</returns>
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
                // 已超时/取消的直接跳过
            }
        }

        if (tcs is null) return false;

        tcs.TrySetResult(response);
        return true;
    }

    // ─── FailAll ──────────────────────────────────────────────────

    /// <summary>
    /// 连接断开时，拒绝所有等待中的请求。
    /// </summary>
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

    // ─── Private Helpers ──────────────────────────────────────────

    private void TryDequeueSpecific(int responseMid, TaskCompletionSource<Mid> target)
    {
        if (!_queues.TryGetValue(responseMid, out var queue)) return;

        lock (queue)
        {
            var snapshot = queue.ToArray();
            queue.Clear();

            foreach (var item in snapshot)
            {
                if (ReferenceEquals(item.Tcs, target)) continue; // 跳过 target
                queue.Enqueue(item);
            }
        }
    }

    // ─── Inner Types ──────────────────────────────────────────────

    private sealed class PendingRequest(TaskCompletionSource<Mid> tcs)
    {
        public TaskCompletionSource<Mid> Tcs { get; } = tcs;
        public bool IsExpired => Tcs.Task.IsCompleted;
    }
}
