using System;

namespace Ddon.Core.Utility
{
    public static class UnixTime
    {
        /// <summary>
        /// 将时间戳转换为日期类型，并格式化
        /// </summary>
        /// <param name="longDateTime"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(long longDateTime)
        {
            //用来格式化long类型时间的,声明的变量
            DateTime start;
            start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddMilliseconds(longDateTime).ToLocalTime();
        }
    }
}
