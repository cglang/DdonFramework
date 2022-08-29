namespace System
{
    /// <summary>
    /// Extension methods for DateTime class.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 仅获取日期部分
        /// </summary>
        /// <returns></returns>
        public static DateTime GetDate(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);
        }
    }
}
