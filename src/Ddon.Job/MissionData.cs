using Ddon.Core.Use.Queue;
using System;
using System.Collections.Generic;

namespace Ddon.Job
{
    internal static class MissionData
    {
        public static readonly Dictionary<Guid, Mission> Missions = new();
        public static readonly DelayQueue<Guid> DelayQueue = new();
    }
}
