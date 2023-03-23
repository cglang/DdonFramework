using System;
using System.Collections.Generic;
using Ddon.Core.Use.Queue;

namespace Ddon.Scheduled;

internal static class ScheduledData
{
    public static readonly Dictionary<Guid, ScheduledInvokeData> Jobs = new();
    public static readonly DelayQueue<Guid> DelayQueue = new();
}
