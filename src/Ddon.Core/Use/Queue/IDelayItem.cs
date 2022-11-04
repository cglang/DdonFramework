using System;

namespace Ddon.Core.Use.Queue
{
    public interface IDelayItem : IComparable
    {
        /// <summary>
        /// 获取剩余延时
        /// </summary>
        /// <returns></returns>
        public TimeSpan DelaySpan { get; }
    }
}
