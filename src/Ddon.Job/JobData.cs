using Ddon.Core.Use.Queue;
using System;
using System.Collections.Generic;

namespace Ddon.Job.old
{
    internal static class JobData
    {
        public static readonly Dictionary<Guid, JobInvokeData> Jobs = new();
        public static readonly DelayQueue<Guid> DelayQueue = new();
    }
}
