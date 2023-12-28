using System;
using System.Collections.Generic;
using Ddon.Core.Use.Queue;

namespace Ddon.Schedule;

internal static class ScheduleData
{
    /// <summary>
    /// 这里面存储所有的后台任务
    /// </summary>
    public static readonly Dictionary<Guid, ScheduleInvokeData> Schedules = new();

    /// <summary>
    /// 后台任务对应的Id会存储到延时队列中，并有对应的到期时间
    /// </summary>
    public static readonly DelayQueue<Guid> DelayQueue = new();
}
