using System;
using System.Collections.Generic;
using Ddon.Core.Use.Queue;

namespace Ddon.Schedule;

internal static class ScheduleData
{
    public static readonly Dictionary<Guid, ScheduleInvokeData> Jobs = new();
    public static readonly DelayQueue<Guid> DelayQueue = new();
}
