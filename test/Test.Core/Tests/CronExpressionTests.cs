using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Ddon.Core.Use.Cronos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cronos.Tests
{
    [TestClass]
    public class CronExpressionTests
    {
        private static readonly bool IsUnix = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
        private static readonly string EasternTimeZoneId = IsUnix ? "America/New_York" : "Eastern Standard Time";
        private static readonly string JordanTimeZoneId = IsUnix ? "Asia/Amman" : "Jordan Standard Time";
        private static readonly string LordHoweTimeZoneId = IsUnix ? "Australia/Lord_Howe" : "Lord Howe Standard Time";
        private static readonly string PacificTimeZoneId = IsUnix ? "America/Santiago" : "Pacific SA Standard Time";

        private static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById(EasternTimeZoneId);
        private static readonly TimeZoneInfo JordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById(JordanTimeZoneId);
        private static readonly TimeZoneInfo LordHoweTimeZone = TimeZoneInfo.FindSystemTimeZoneById(LordHoweTimeZoneId);
        private static readonly TimeZoneInfo PacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById(PacificTimeZoneId);

        private static readonly DateTime Today = new(2016, 12, 09);

        private static readonly CronExpression MinutelyExpression = CronExpression.Parse("* * * * *");

        #region 学习使用

        [TestMethod]
        [DataRow("0 * * * * ?", "2022-10-10 00:00:00 +00:00", "2022-10-10 00:00:00 +00:00", true)]
        [DataRow("0 * * * * ?", "2022-10-10 00:00:00 +00:00", "2022-10-10 00:01:00 +00:00", false)]
        [DataRow("0 * * * * ?", "2022-10-10 00:01:00 +00:00", "2022-10-10 00:02:00 +00:00", false)]
        [DataRow("0 0 6 * * ?", "2022-10-10 00:00:00 +00:00", "2022-10-10 06:00:00 +00:00", false)]
        [DataRow("0 0 6 * * ?", "2022-10-10 06:00:00 +00:00", "2022-10-11 06:00:00 +00:00", false)]
        [DataRow("0 0 6 * * 1-5", "2022-12-05 00:00:00 +00:00", "2022-12-05 06:00:00 +00:00", false)]
        [DataRow("0 0 6 * * 1-5", "2022-12-10 00:00:00 +00:00", "2022-12-12 06:00:00 +00:00", false)]
        public void LearnTest(string cronExpression, string fromString, string expectedString, bool inclusive)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, TimeZoneInfo.Utc, inclusive);

            Assert.AreEqual(expectedInstant, executed);
        }

        [TestMethod]
        [DataRow("0 * * * * ?", "2022-10-10 00:00:00 +00:00", "2022-10-10 00:00:00 +00:00", true)]
        public void Learn2Test(string cronExpression, string fromString, string expectedString, bool inclusive)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, TimeZoneInfo.Local, inclusive);

            Assert.AreEqual(expectedInstant, executed);
        }

        #endregion


        [TestMethod]
        [DataRow("*	*	* * * *")]        // Handle tabs.
        [DataRow(" 	*	*	* * * *    ")]// Handle white spaces at the beginning and end of expression.        
        [DataRow("  @every_second ")]     // Handle white spaces for macros.
        public void HandleWhiteSpaces(string cronExpression)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            var from = new DateTime(2016, 03, 18, 12, 0, 0, DateTimeKind.Utc);
            var result = expression.GetNextOccurrence(from, inclusive: true);
            Assert.AreEqual(from, result);
        }

        [TestMethod]
        public void Parse_ThrowAnException_WhenCronExpressionIsNull()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => CronExpression.Parse(string.Empty));

            Assert.AreEqual("expression", exception.ParamName);
        }

        [TestMethod]
        public void Parse_ThrowAnException_WhenCronExpressionIsEmpty()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => CronExpression.Parse(""));

            Assert.AreEqual("expression", exception.ParamName);
        }

        #region Cron表达式无效时 引发异常

        [TestMethod]
        // Second field is invalid.
        [DataRow("-1   * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("-    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("5-   * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow(",    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow(",1   * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("/    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("*/   * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("1/   * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("1/0  * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("1/60 * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("1/k  * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("1k   * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("#    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("*#1  * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("0#2  * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("L    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("l    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("W    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("w    * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("LW   * * * * *", CronFormat.IncludeSeconds, "Seconds")]
        [DataRow("lw   * * * * *", CronFormat.IncludeSeconds, "Seconds")]

        // 2147483648 = Int32.MaxValue + 1
        [DataRow("1/2147483648 * * * * *", CronFormat.IncludeSeconds, "Seconds")]

        // Minute field is invalid.
        [DataRow("60    * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("-1    * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("-     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("7-    * * * *", CronFormat.Standard, "Minutes")]
        [DataRow(",     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow(",1    * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("*/    * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("/     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("1/    * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("1/0   * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("1/60  * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("1/k   * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("1k    * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("#     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("*#1   * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("5#3   * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("L     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("l     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("W     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("w     * * * *", CronFormat.Standard, "Minutes")]
        [DataRow("lw    * * * *", CronFormat.Standard, "Minutes")]

        [DataRow("* 60    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* -1    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* -     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* 7-    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* ,     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* ,1    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* */    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* /     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* 1/    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* 1/0   * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* 1/60  * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* 1/k   * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* 1k    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* #     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* *#1   * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* 5#3   * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* L     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* l     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* W     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* w     * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* LW    * * * *", CronFormat.IncludeSeconds, "Minutes")]
        [DataRow("* lw    * * * *", CronFormat.IncludeSeconds, "Minutes")]

        // Hour field is invalid.
        [DataRow("* 25   * * *", CronFormat.Standard, "Hours")]
        [DataRow("* -1   * * *", CronFormat.Standard, "Hours")]
        [DataRow("* -    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* 0-   * * *", CronFormat.Standard, "Hours")]
        [DataRow("* ,    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* ,1   * * *", CronFormat.Standard, "Hours")]
        [DataRow("* /    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* 1/   * * *", CronFormat.Standard, "Hours")]
        [DataRow("* 1/0  * * *", CronFormat.Standard, "Hours")]
        [DataRow("* 1/24 * * *", CronFormat.Standard, "Hours")]
        [DataRow("* 1/k  * * *", CronFormat.Standard, "Hours")]
        [DataRow("* 1k   * * *", CronFormat.Standard, "Hours")]
        [DataRow("* #    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* *#2  * * *", CronFormat.Standard, "Hours")]
        [DataRow("* 10#1 * * *", CronFormat.Standard, "Hours")]
        [DataRow("* L    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* l    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* W    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* w    * * *", CronFormat.Standard, "Hours")]
        [DataRow("* LW   * * *", CronFormat.Standard, "Hours")]
        [DataRow("* lw   * * *", CronFormat.Standard, "Hours")]

        [DataRow("* * 25   * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * -1   * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * -    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * 0-   * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * ,    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * ,1   * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * /    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * 1/   * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * 1/0  * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * 1/24 * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * 1/k  * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * 1k   * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * #    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * *#2  * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * 10#1 * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * L    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * l    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * W    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * w    * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * LW   * * *", CronFormat.IncludeSeconds, "Hours")]
        [DataRow("* * lw   * * *", CronFormat.IncludeSeconds, "Hours")]

        // Day of month field is invalid.
        [DataRow("* * 32     *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 10-32  *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 31-32  *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * -1     *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * -      *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 8-     *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * ,      *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * ,1     *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * /      *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1/     *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1/0    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1/32   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1/k    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1m     *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * T      *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * MON    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * mon    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * #      *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * *#3    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 4#1    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * W      *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * w      *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1-2W   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1-2w   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1,2W   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1,2w   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1/2W   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1/2w   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1-2/2W *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1-2/2w *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1LW    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * 1lw    *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * L-31   *  *", CronFormat.Standard, "Days of month")]
        [DataRow("* * l-31   *  *", CronFormat.Standard, "Days of month")]

        [DataRow("* * * 32     *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 10-32  *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 31-32  *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * -1     *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * -      *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 8-     *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * ,      *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * ,1     *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * /      *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1/     *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1/0    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1/32   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1/k    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1m     *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * T      *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * MON    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * mon    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * #      *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * *#3    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 4#1    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * W      *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * w      *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1-2W   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1-2w   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1,2W   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1,2w   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1/2W   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1/2w   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1-2/2W *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1-2/2w *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1LW    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * 1lw    *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * L-31   *  *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * l-31   *  *", CronFormat.IncludeSeconds, "Days of month")]

        // Month field is invalid.
        [DataRow("* * * 13   *", CronFormat.Standard, "Months")]
        [DataRow("* * * -1   *", CronFormat.Standard, "Months")]
        [DataRow("* * * -    *", CronFormat.Standard, "Months")]
        [DataRow("* * * 2-   *", CronFormat.Standard, "Months")]
        [DataRow("* * * ,    *", CronFormat.Standard, "Months")]
        [DataRow("* * * ,1   *", CronFormat.Standard, "Months")]
        [DataRow("* * * /    *", CronFormat.Standard, "Months")]
        [DataRow("* * * */   *", CronFormat.Standard, "Months")]
        [DataRow("* * * 1/   *", CronFormat.Standard, "Months")]
        [DataRow("* * * 1/0  *", CronFormat.Standard, "Months")]
        [DataRow("* * * 1/13 *", CronFormat.Standard, "Months")]
        [DataRow("* * * 1/k  *", CronFormat.Standard, "Months")]
        [DataRow("* * * 1k   *", CronFormat.Standard, "Months")]
        [DataRow("* * * #    *", CronFormat.Standard, "Months")]
        [DataRow("* * * *#1  *", CronFormat.Standard, "Months")]
        [DataRow("* * * */2# *", CronFormat.Standard, "Months")]
        [DataRow("* * * 2#2  *", CronFormat.Standard, "Months")]
        [DataRow("* * * L    *", CronFormat.Standard, "Months")]
        [DataRow("* * * l    *", CronFormat.Standard, "Months")]
        [DataRow("* * * W    *", CronFormat.Standard, "Months")]
        [DataRow("* * * w    *", CronFormat.Standard, "Months")]
        [DataRow("* * * LW   *", CronFormat.Standard, "Months")]
        [DataRow("* * * lw   *", CronFormat.Standard, "Months")]

        [DataRow("* * * * 13   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * -1   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * -    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * 2-   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * ,    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * ,1   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * /    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * */   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * 1/   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * 1/0  *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * 1/13 *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * 1/k  *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * 1k   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * #    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * *#1  *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * */2# *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * 2#2  *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * L    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * l    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * W    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * w    *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * LW   *", CronFormat.IncludeSeconds, "Months")]
        [DataRow("* * * * lw   *", CronFormat.IncludeSeconds, "Months")]

        // Day of week field is invalid.
        [DataRow("* * * * 8      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * -1     ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * -      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * 3-     ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * ,      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * ,1     ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * /      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * */     ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * 1/     ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * 1/0    ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * 1/8    ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * #      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * 0#     ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * 5#6    ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * SUN#6  ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * sun#6  ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * SUN#050", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * sun#050", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * 0#0    ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * SUT    ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * sut    ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * SU0    ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * SUNDAY ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * L      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * l      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * W      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * w      ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * LW     ", CronFormat.Standard, "Days of week")]
        [DataRow("* * * * lw     ", CronFormat.Standard, "Days of week")]

        [DataRow("* * * * * 8      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * -1     ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * -      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * 3-     ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * ,      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * ,1     ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * /      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * */     ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * 1/     ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * 1/0    ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * 1/8    ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * #      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * 0#     ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * 5#6    ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * SUN#6  ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * sun#6  ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * SUN#050", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * sun#050", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * 0#0    ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * SUT    ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * sut    ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * SU0    ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * SUNDAY ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * L      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * l      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * W      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * w      ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * LW     ", CronFormat.IncludeSeconds, "Days of week")]
        [DataRow("* * * * * lw     ", CronFormat.IncludeSeconds, "Days of week")]

        // Fields count is invalid.
        [DataRow("* * *        ", CronFormat.Standard, "Months")]
        [DataRow("* * * * * * *", CronFormat.Standard, "")]

        [DataRow("* * * *", CronFormat.IncludeSeconds, "Days of month")]
        [DataRow("* * * * * * *", CronFormat.IncludeSeconds, "")]

        // Macro is invalid.
        [DataRow("@", CronFormat.Standard, "")]

        // ReSharper disable StringLiteralTypo
        [DataRow("@invalid        ", CronFormat.Standard, "")]
        [DataRow("          @yearl", CronFormat.Standard, "")]
        [DataRow("@yearl          ", CronFormat.Standard, "")]
        [DataRow("@yearly !       ", CronFormat.Standard, "")]
        [DataRow("@every_hour     ", CronFormat.Standard, "")]
        [DataRow("@@daily         ", CronFormat.Standard, "")]
        [DataRow("@yeannually     ", CronFormat.Standard, "")]
        [DataRow("@yweekly        ", CronFormat.Standard, "")]
        [DataRow("@ymonthly       ", CronFormat.Standard, "")]
        [DataRow("@ydaily         ", CronFormat.Standard, "")]
        [DataRow("@ymidnight      ", CronFormat.Standard, "")]
        [DataRow("@yhourly        ", CronFormat.Standard, "")]
        [DataRow("@yevery_second  ", CronFormat.Standard, "")]
        [DataRow("@yevery_minute  ", CronFormat.Standard, "")]
        [DataRow("@every_minsecond", CronFormat.Standard, "")]
        [DataRow("@annuall        ", CronFormat.Standard, "")]
        [DataRow("@dail           ", CronFormat.Standard, "")]
        [DataRow("@hour           ", CronFormat.Standard, "")]
        [DataRow("@midn           ", CronFormat.Standard, "")]
        [DataRow("@week           ", CronFormat.Standard, "")]

        [DataRow("@", CronFormat.IncludeSeconds, "")]

        [DataRow("@invalid        ", CronFormat.IncludeSeconds, "")]
        [DataRow("          @yearl", CronFormat.IncludeSeconds, "")]
        [DataRow("@yearl          ", CronFormat.IncludeSeconds, "")]
        [DataRow("@yearly !       ", CronFormat.IncludeSeconds, "")]
        [DataRow("@dai            ", CronFormat.IncludeSeconds, "")]
        [DataRow("@a              ", CronFormat.IncludeSeconds, "")]
        [DataRow("@every_hour     ", CronFormat.IncludeSeconds, "")]
        [DataRow("@everysecond    ", CronFormat.IncludeSeconds, "")]
        [DataRow("@@daily         ", CronFormat.IncludeSeconds, "")]
        [DataRow("@yeannually     ", CronFormat.IncludeSeconds, "")]
        [DataRow("@yweekly        ", CronFormat.IncludeSeconds, "")]
        [DataRow("@ymonthly       ", CronFormat.IncludeSeconds, "")]
        [DataRow("@ydaily         ", CronFormat.IncludeSeconds, "")]
        [DataRow("@ymidnight      ", CronFormat.IncludeSeconds, "")]
        [DataRow("@yhourly        ", CronFormat.IncludeSeconds, "")]
        [DataRow("@yevery_second  ", CronFormat.IncludeSeconds, "")]
        [DataRow("@yevery_minute  ", CronFormat.IncludeSeconds, "")]
        [DataRow("@every_minsecond", CronFormat.IncludeSeconds, "")]
        [DataRow("@annuall        ", CronFormat.IncludeSeconds, "")]
        [DataRow("@dail           ", CronFormat.IncludeSeconds, "")]
        [DataRow("@hour           ", CronFormat.IncludeSeconds, "")]
        [DataRow("@midn           ", CronFormat.IncludeSeconds, "")]
        [DataRow("@week           ", CronFormat.IncludeSeconds, "")]

        [DataRow("60 * * * *", CronFormat.Standard, "between 0 and 59")]
        [DataRow("*/60 * * * *", CronFormat.Standard, "between 1 and 59")]
        public void Parse_ThrowsCronFormatException_WhenCronExpressionIsInvalid(string cronExpression, CronFormat format, string invalidField)
        {
            var exception = Assert.ThrowsException<CronFormatException>(() => CronExpression.Parse(cronExpression, format));
            Assert.IsTrue(exception.Message.Contains(invalidField));
        }

        #endregion

        #region 正确的宏表达式

        [TestMethod]
        [DataRow("  @yearly      ", CronFormat.Standard)]
        [DataRow("  @YEARLY      ", CronFormat.Standard)]
        [DataRow("  @annually    ", CronFormat.Standard)]
        [DataRow("  @ANNUALLY    ", CronFormat.Standard)]
        [DataRow("  @monthly     ", CronFormat.Standard)]
        [DataRow("  @MONTHLY     ", CronFormat.Standard)]
        [DataRow("  @weekly      ", CronFormat.Standard)]
        [DataRow("  @WEEKLY      ", CronFormat.Standard)]
        [DataRow("  @daily       ", CronFormat.Standard)]
        [DataRow("  @DAILY       ", CronFormat.Standard)]
        [DataRow("  @midnight    ", CronFormat.Standard)]
        [DataRow("  @MIDNIGHT    ", CronFormat.Standard)]
        [DataRow("  @every_minute", CronFormat.Standard)]
        [DataRow("  @EVERY_MINUTE", CronFormat.Standard)]
        [DataRow("  @every_second", CronFormat.Standard)]
        [DataRow("  @EVERY_SECOND", CronFormat.Standard)]

        [DataRow("  @yearly      ", CronFormat.IncludeSeconds)]
        [DataRow("  @YEARLY      ", CronFormat.IncludeSeconds)]
        [DataRow("  @annually    ", CronFormat.IncludeSeconds)]
        [DataRow("  @ANNUALLY    ", CronFormat.IncludeSeconds)]
        [DataRow("  @monthly     ", CronFormat.IncludeSeconds)]
        [DataRow("  @MONTHLY     ", CronFormat.IncludeSeconds)]
        [DataRow("  @weekly      ", CronFormat.IncludeSeconds)]
        [DataRow("  @WEEKLY      ", CronFormat.IncludeSeconds)]
        [DataRow("  @daily       ", CronFormat.IncludeSeconds)]
        [DataRow("  @DAILY       ", CronFormat.IncludeSeconds)]
        [DataRow("  @midnight    ", CronFormat.IncludeSeconds)]
        [DataRow("  @MIDNIGHT    ", CronFormat.IncludeSeconds)]
        [DataRow("  @every_minute", CronFormat.IncludeSeconds)]
        [DataRow("  @EVERY_MINUTE", CronFormat.IncludeSeconds)]
        [DataRow("  @every_second", CronFormat.IncludeSeconds)]
        [DataRow("  @EVERY_SECOND", CronFormat.IncludeSeconds)]
        public void Parse_DoesNotThrowAnException_WhenExpressionIsMacro(string cronExpression, CronFormat format)
        {
            CronExpression.Parse(cronExpression, format);
        }

        #endregion

        #region 与时间时区有关
        [TestMethod]
        [DataRow(DateTimeKind.Unspecified, false)]
        [DataRow(DateTimeKind.Unspecified, true)]
        [DataRow(DateTimeKind.Local, false)]
        [DataRow(DateTimeKind.Local, true)]
        public void GetNextOccurrence_ThrowsAnException_WhenFromHasAWrongKind(DateTimeKind kind, bool inclusive)
        {
            var from = new DateTime(2017, 03, 22, 0, 0, 0, kind);
            var exception = Assert.ThrowsException<ArgumentException>(() => MinutelyExpression.GetNextOccurrence(from, TimeZoneInfo.Local, inclusive));
            Assert.AreEqual("fromUtc", exception.ParamName);
        }

        [TestMethod]
        [DataRow(DateTimeKind.Unspecified, false)]
        [DataRow(DateTimeKind.Unspecified, true)]
        [DataRow(DateTimeKind.Local, false)]
        [DataRow(DateTimeKind.Local, true)]
        public void GetNextOccurrence_ThrowsAnException_WhenFromDoesNotHaveUtcKind(DateTimeKind kind, bool inclusive)
        {
            var from = new DateTime(2017, 03, 15, 0, 0, 0, kind);
            var exception = Assert.ThrowsException<ArgumentException>(() => MinutelyExpression.GetNextOccurrence(from, inclusive));
            Assert.AreEqual("fromUtc", exception.ParamName);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void GetNextOccurrence_ReturnsDateTimeWithUtcKind(bool inclusive)
        {
            var from = new DateTime(2017, 03, 22, 9, 32, 0, DateTimeKind.Utc);
            var occurrence = MinutelyExpression.GetNextOccurrence(from, inclusive);
            Assert.AreEqual(DateTimeKind.Utc, occurrence?.Kind);
        }
        #endregion

        #region 获取下一次执行时间是否是正确的时间
        [TestMethod]

        // Basic facts.
        [DataRow("* * * * * *", "17:35:00", "17:35:00")]

        // Second specified.
        [DataRow("20    * * * * *", "17:35:00", "17:35:20")]
        [DataRow("20    * * * * *", "17:35:20", "17:35:20")]
        [DataRow("20    * * * * *", "17:35:40", "17:36:20")]
        [DataRow("10-30 * * * * *", "17:35:09", "17:35:10")]
        [DataRow("10-30 * * * * *", "17:35:10", "17:35:10")]
        [DataRow("10-30 * * * * *", "17:35:20", "17:35:20")]
        [DataRow("10-30 * * * * *", "17:35:30", "17:35:30")]
        [DataRow("10-30 * * * * *", "17:35:31", "17:36:10")]
        [DataRow("*/20  * * * * *", "17:35:00", "17:35:00")]
        [DataRow("*/20  * * * * *", "17:35:11", "17:35:20")]
        [DataRow("*/20  * * * * *", "17:35:20", "17:35:20")]
        [DataRow("*/20  * * * * *", "17:35:25", "17:35:40")]
        [DataRow("*/20  * * * * *", "17:35:59", "17:36:00")]
        [DataRow("10/5  * * * * *", "17:35:00", "17:35:10")]
        [DataRow("10/5  * * * * *", "17:35:12", "17:35:15")]
        [DataRow("10/5  * * * * *", "17:35:59", "17:36:10")]
        [DataRow("0     * * * * *", "17:35:59", "17:36:00")]
        [DataRow("0     * * * * *", "17:59:59", "18:00:00")]

        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:01", "17:35:05")]
        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:06", "17:35:06")]
        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:18", "17:35:19")]
        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:19", "17:35:19")]
        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:20", "17:35:20")]
        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:21", "17:35:35")]
        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:36", "17:35:36")]
        [DataRow("5-8,19,20,35-41 * * * * *", "17:35:42", "17:36:05")]

        [DataRow("55-5 * * * * ?", "17:35:42", "17:35:55")]
        [DataRow("55-5 * * * * ?", "17:35:57", "17:35:57")]
        [DataRow("55-5 * * * * ?", "17:35:59", "17:35:59")]
        [DataRow("55-5 * * * * ?", "17:36:00", "17:36:00")]
        [DataRow("55-5 * * * * ?", "17:36:05", "17:36:05")]
        [DataRow("55-5 * * * * ?", "17:36:06", "17:36:55")]

        [DataRow("57-5/3 * * * * ?", "17:36:06", "17:36:57")]
        [DataRow("57-5/3 * * * * ?", "17:36:58", "17:37:00")]
        [DataRow("57-5/3 * * * * ?", "17:37:01", "17:37:03")]
        [DataRow("57-5/3 * * * * ?", "17:37:04", "17:37:57")]

        [DataRow("59-58 * * * * ?", "17:37:04", "17:37:04")]
        [DataRow("59-58 * * * * ?", "17:37:58", "17:37:58")]
        [DataRow("59-58 * * * * ?", "17:37:59", "17:37:59")]
        [DataRow("59-58 * * * * ?", "17:38:00", "17:38:00")]

        // Minute specified.

        [DataRow("* 12    * * * *", "15:05", "15:12")]
        [DataRow("* 12    * * * *", "15:12", "15:12")]
        [DataRow("* 12    * * * *", "15:59", "16:12")]
        [DataRow("* 31-39 * * * *", "15:00", "15:31")]
        [DataRow("* 31-39 * * * *", "15:30", "15:31")]
        [DataRow("* 31-39 * * * *", "15:31", "15:31")]
        [DataRow("* 31-39 * * * *", "15:39", "15:39")]
        [DataRow("* 31-39 * * * *", "15:59", "16:31")]
        [DataRow("* */20  * * * *", "15:00", "15:00")]
        [DataRow("* */20  * * * *", "15:10", "15:20")]
        [DataRow("* */20  * * * *", "15:59", "16:00")]
        [DataRow("* 10/5  * * * *", "15:00", "15:10")]
        [DataRow("* 10/5  * * * *", "15:14", "15:15")]
        [DataRow("* 10/5  * * * *", "15:59", "16:10")]
        [DataRow("* 0     * * * *", "15:59", "16:00")]

        [DataRow("* 5-8,19,20,35-41 * * * *", "15:01", "15:05")]
        [DataRow("* 5-8,19,20,35-41 * * * *", "15:06", "15:06")]
        [DataRow("* 5-8,19,20,35-41 * * * *", "15:18", "15:19")]
        [DataRow("* 5-8,19,20,35-41 * * * *", "15:19", "15:19")]
        [DataRow("* 5-8,19,20,35-41 * * * *", "15:20", "15:20")]
        [DataRow("* 5-8,19,20,35-41 * * * *", "15:21", "15:35")]
        [DataRow("* 5-8,19,20,35-41 * * * *", "15:36", "15:36")]
        [DataRow("* 5-8,19,20,35-41 * * * *", "15:42", "16:05")]

        [DataRow("* 51-4 * * * *", "17:35", "17:51")]
        [DataRow("* 51-4 * * * *", "17:51", "17:51")]
        [DataRow("* 51-4 * * * *", "17:55", "17:55")]
        [DataRow("* 51-4 * * * *", "17:59", "17:59")]
        [DataRow("* 51-4 * * * *", "18:00", "18:00")]
        [DataRow("* 51-4 * * * *", "18:04", "18:04")]
        [DataRow("* 51-4 * * * *", "18:05", "18:51")]

        [DataRow("* 56-4/4 * * * *", "17:55", "17:56")]
        [DataRow("* 56-4/4 * * * *", "17:57", "18:00")]
        [DataRow("* 56-4/4 * * * *", "18:01", "18:04")]
        [DataRow("* 56-4/4 * * * *", "18:05", "18:56")]

        [DataRow("* 45-44 * * * *", "18:45", "18:45")]
        [DataRow("* 45-44 * * * *", "18:55", "18:55")]
        [DataRow("* 45-44 * * * *", "18:59", "18:59")]
        [DataRow("* 45-44 * * * *", "19:00", "19:00")]
        [DataRow("* 45-44 * * * *", "19:44", "19:44")]

        // Hour specified.

        [DataRow("* * 11   * * *", "10:59", "11:00")]
        [DataRow("* * 11   * * *", "11:30", "11:30")]
        [DataRow("* * 3-22 * * *", "01:40", "03:00")]
        [DataRow("* * 3-22 * * *", "11:40", "11:40")]
        [DataRow("* * */2  * * *", "00:00", "00:00")]
        [DataRow("* * */2  * * *", "01:00", "02:00")]
        [DataRow("* * 4/5  * * *", "00:45", "04:00")]
        [DataRow("* * 4/5  * * *", "04:14", "04:14")]
        [DataRow("* * 4/5  * * *", "05:00", "09:00")]

        [DataRow("* * 3-5,10,11,13-17 * * *", "01:55", "03:00")]
        [DataRow("* * 3-5,10,11,13-17 * * *", "04:55", "04:55")]
        [DataRow("* * 3-5,10,11,13-17 * * *", "06:10", "10:00")]
        [DataRow("* * 3-5,10,11,13-17 * * *", "10:55", "10:55")]
        [DataRow("* * 3-5,10,11,13-17 * * *", "11:25", "11:25")]
        [DataRow("* * 3-5,10,11,13-17 * * *", "12:30", "13:00")]
        [DataRow("* * 3-5,10,11,13-17 * * *", "17:30", "17:30")]

        [DataRow("* * 23-3/2 * * *", "17:30", "23:00")]
        [DataRow("* * 23-3/2 * * *", "00:30", "01:00")]
        [DataRow("* * 23-3/2 * * *", "02:00", "03:00")]
        [DataRow("* * 23-3/2 * * *", "04:00", "23:00")]

        [DataRow("* * 23-22 * * *", "22:10", "22:10")]
        [DataRow("* * 23-22 * * *", "23:10", "23:10")]
        [DataRow("* * 23-22 * * *", "00:10", "00:10")]
        [DataRow("* * 23-22 * * *", "07:10", "07:10")]

        // Day of month specified.

        [DataRow("* * * 9     * *", "2016-11-01", "2016-11-09")]
        [DataRow("* * * 9     * *", "2016-11-09", "2016-11-09")]
        [DataRow("* * * 09    * *", "2016-11-10", "2016-12-09")]
        [DataRow("* * * */4   * *", "2016-12-01", "2016-12-01")]
        [DataRow("* * * */4   * *", "2016-12-02", "2016-12-05")]
        [DataRow("* * * */4   * *", "2016-12-06", "2016-12-09")]
        [DataRow("* * * */3   * *", "2016-12-02", "2016-12-04")]
        [DataRow("* * * 10,20 * *", "2016-12-09", "2016-12-10")]
        [DataRow("* * * 10,20 * *", "2016-12-12", "2016-12-20")]
        [DataRow("* * * 16-23 * *", "2016-12-01", "2016-12-16")]
        [DataRow("* * * 16-23 * *", "2016-12-16", "2016-12-16")]
        [DataRow("* * * 16-23 * *", "2016-12-18", "2016-12-18")]
        [DataRow("* * * 16-23 * *", "2016-12-23", "2016-12-23")]
        [DataRow("* * * 16-23 * *", "2016-12-24", "2017-01-16")]

        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-01", "2016-12-05")]
        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-05", "2016-12-05")]
        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-06", "2016-12-06")]
        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-08", "2016-12-08")]
        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-09", "2016-12-19")]
        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-20", "2016-12-20")]
        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-21", "2016-12-28")]
        [DataRow("* * * 5-8,19,20,28-29 * *", "2016-12-30", "2017-01-05")]
        [DataRow("* * * 5-8,19,20,29-30 * *", "2017-02-27", "2017-03-05")]

        [DataRow("* * * 30-31 * *", "2016-02-27", "2016-03-30")]
        [DataRow("* * * 30-31 * *", "2017-02-27", "2017-03-30")]
        [DataRow("* * * 31    * *", "2017-04-27", "2017-05-31")]

        [DataRow("* * * 20-5/5 * *", "2017-05-19", "2017-05-20")]
        [DataRow("* * * 20-5/5 * *", "2017-05-21", "2017-05-25")]
        [DataRow("* * * 20-5/5 * *", "2017-05-26", "2017-05-30")]
        [DataRow("* * * 20-5/5 * *", "2017-06-01", "2017-06-04")]
        [DataRow("* * * 20-5/5 * *", "2017-06-05", "2017-06-20")]

        [DataRow("* * * 20-5/5 * *", "2017-07-01", "2017-07-04")]

        [DataRow("* * * 20-5/5 * *", "2018-02-26", "2018-03-04")]

        // Month specified.

        [DataRow("* * * * 11      *", "2016-10-09", "2016-11-01")]
        [DataRow("* * * * 11      *", "2016-11-02", "2016-11-02")]
        [DataRow("* * * * 11      *", "2016-12-02", "2017-11-01")]
        [DataRow("* * * * 3,9     *", "2016-01-09", "2016-03-01")]
        [DataRow("* * * * 3,9     *", "2016-06-09", "2016-09-01")]
        [DataRow("* * * * 3,9     *", "2016-10-09", "2017-03-01")]
        [DataRow("* * * * 5-11    *", "2016-01-01", "2016-05-01")]
        [DataRow("* * * * 5-11    *", "2016-05-07", "2016-05-07")]
        [DataRow("* * * * 5-11    *", "2016-07-12", "2016-07-12")]
        [DataRow("* * * * 05-11   *", "2016-12-13", "2017-05-01")]
        [DataRow("* * * * DEC     *", "2016-08-09", "2016-12-01")]
        [DataRow("* * * * mar-dec *", "2016-02-09", "2016-03-01")]
        [DataRow("* * * * mar-dec *", "2016-04-09", "2016-04-09")]
        [DataRow("* * * * mar-dec *", "2016-12-09", "2016-12-09")]
        [DataRow("* * * * */4     *", "2016-01-09", "2016-01-09")]
        [DataRow("* * * * */4     *", "2016-02-09", "2016-05-01")]
        [DataRow("* * * * */3     *", "2016-12-09", "2017-01-01")]
        [DataRow("* * * * */5     *", "2016-12-09", "2017-01-01")]
        [DataRow("* * * * APR-NOV *", "2016-12-09", "2017-04-01")]

        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-01-01", "2016-02-01")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-02-10", "2016-02-10")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-03-01", "2016-03-01")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-05-20", "2016-06-01")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-06-10", "2016-06-10")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-07-05", "2016-07-05")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-08-15", "2016-09-01")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-11-25", "2016-11-25")]
        [DataRow("* * * * 2-4,JUN,7,SEP-nov *", "2016-12-01", "2017-02-01")]

        [DataRow("* * * * 12-2 *", "2016-05-19", "2016-12-01")]
        [DataRow("* * * * 12-2 *", "2017-01-19", "2017-01-19")]
        [DataRow("* * * * 12-2 *", "2017-02-19", "2017-02-19")]
        [DataRow("* * * * 12-2 *", "2017-03-19", "2017-12-01")]

        [DataRow("* * * * 9-8/3 *", "2016-07-19", "2016-09-01")]
        [DataRow("* * * * 9-8/3 *", "2016-10-19", "2016-12-01")]
        [DataRow("* * * * 9-8/3 *", "2017-01-19", "2017-03-01")]
        [DataRow("* * * * 9-8/3 *", "2017-04-19", "2017-06-01")]

        // Day of week specified.

        // Monday        Tuesday       Wednesday     Thursday      Friday        Saturday      Sunday
        //                                           2016-12-01    2016-12-02    2016-12-03    2016-12-04
        // 2016-12-05    2016-12-06    2016-12-07    2016-12-08    2016-12-09    2016-12-10    2016-12-11
        // 2016-12-12    2016-12-13    2016-12-14    2016-12-15    2016-12-16    2016-12-17    2016-12-18

        [DataRow("* * * * * 5      ", "2016-12-07", "2016-12-09")]
        [DataRow("* * * * * 5      ", "2016-12-09", "2016-12-09")]
        [DataRow("* * * * * 05     ", "2016-12-10", "2016-12-16")]
        [DataRow("* * * * * 3,5,7  ", "2016-12-09", "2016-12-09")]
        [DataRow("* * * * * 3,5,7  ", "2016-12-10", "2016-12-11")]
        [DataRow("* * * * * 3,5,7  ", "2016-12-12", "2016-12-14")]
        [DataRow("* * * * * 4-7    ", "2016-12-08", "2016-12-08")]
        [DataRow("* * * * * 4-7    ", "2016-12-10", "2016-12-10")]
        [DataRow("* * * * * 4-7    ", "2016-12-11", "2016-12-11")]
        [DataRow("* * * * * 4-07   ", "2016-12-12", "2016-12-15")]
        [DataRow("* * * * * FRI    ", "2016-12-08", "2016-12-09")]
        [DataRow("* * * * * tue/2  ", "2016-12-09", "2016-12-10")]
        [DataRow("* * * * * tue/2  ", "2016-12-11", "2016-12-13")]
        [DataRow("* * * * * FRI/3  ", "2016-12-03", "2016-12-09")]
        [DataRow("* * * * * thu-sat", "2016-12-04", "2016-12-08")]
        [DataRow("* * * * * thu-sat", "2016-12-09", "2016-12-09")]
        [DataRow("* * * * * thu-sat", "2016-12-10", "2016-12-10")]
        [DataRow("* * * * * thu-sat", "2016-12-12", "2016-12-15")]
        [DataRow("* * * * * */5    ", "2016-12-08", "2016-12-09")]
        [DataRow("* * * * * */5    ", "2016-12-10", "2016-12-11")]
        [DataRow("* * * * * */5    ", "2016-12-12", "2016-12-16")]
        [DataRow("* * * ? * thu-sun", "2016-12-09", "2016-12-09")]

        [DataRow("* * * ? * sat-tue", "2016-12-10", "2016-12-10")]
        [DataRow("* * * ? * sat-tue", "2016-12-11", "2016-12-11")]
        [DataRow("* * * ? * sat-tue", "2016-12-12", "2016-12-12")]
        [DataRow("* * * ? * sat-tue", "2016-12-13", "2016-12-13")]
        [DataRow("* * * ? * sat-tue", "2016-12-14", "2016-12-17")]

        [DataRow("* * * ? * sat-tue/2", "2016-12-10", "2016-12-10")]
        [DataRow("* * * ? * sat-tue/2", "2016-12-11", "2016-12-12")]
        [DataRow("* * * ? * sat-tue/2", "2016-12-12", "2016-12-12")]
        [DataRow("* * * ? * sat-tue/2", "2016-12-13", "2016-12-17")]

        [DataRow("00 00 00 11 12 0  ", "2016-12-07", "2016-12-11")]
        [DataRow("00 00 00 11 12 7  ", "2016-12-09", "2016-12-11")]
        [DataRow("00 00 00 11 12 SUN", "2016-12-10", "2016-12-11")]
        [DataRow("00 00 00 11 12 sun", "2016-12-09", "2016-12-11")]

        // All fields are specified.

        [DataRow("54    47    17    09   12    5    ", "2016-10-01 00:00:00", "2016-12-09 17:47:54")]
        [DataRow("54    47    17    09   DEC   FRI  ", "2016-07-05 00:00:00", "2016-12-09 17:47:54")]
        [DataRow("50-56 40-50 15-20 5-10 11,12 5,6,7", "2016-12-01 00:00:00", "2016-12-09 15:40:50")]
        [DataRow("50-56 40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 15:40:53", "2016-12-09 15:40:53")]
        [DataRow("50-56 40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 15:40:57", "2016-12-09 15:41:50")]
        [DataRow("50-56 40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 15:45:56", "2016-12-09 15:45:56")]
        [DataRow("50-56 40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 15:51:56", "2016-12-09 16:40:50")]
        [DataRow("50-56 40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 21:50:56", "2016-12-10 15:40:50")]
        [DataRow("50-56 40-50 15-20 5-10 11,12 5,6,7", "2016-12-11 21:50:56", "2017-11-05 15:40:50")]

        // Friday the thirteenth.

        [DataRow("00    05    18    13   01    05   ", "2016-01-01 00:00:00", "2017-01-13 18:05:00")]
        [DataRow("00    05    18    13   *     05   ", "2016-01-01 00:00:00", "2016-05-13 18:05:00")]
        [DataRow("00    05    18    13   *     05   ", "2016-09-01 00:00:00", "2017-01-13 18:05:00")]
        [DataRow("00    05    18    13   *     05   ", "2017-02-01 00:00:00", "2017-10-13 18:05:00")]

        // Handle moving to next second, minute, hour, month, year.

        [DataRow("0 * * * * *", "2017-01-14 12:58:59", "2017-01-14 12:59:00")]

        [DataRow("0 0 * * * *", "2017-01-14 12:59", "2017-01-14 13:00")]
        [DataRow("0 0 0 * * *", "2017-01-14 23:00", "2017-01-15 00:00")]

        [DataRow("0 0 0 1 * *", "2016-02-10 00:00", "2016-03-01 00:00")]
        [DataRow("0 0 0 1 * *", "2017-02-10 00:00", "2017-03-01 00:00")]
        [DataRow("0 0 0 1 * *", "2017-04-10 00:00", "2017-05-01 00:00")]
        [DataRow("0 0 0 1 * *", "2017-01-30 00:00", "2017-02-01 00:00")]
        [DataRow("0 0 0 * * *", "2017-12-31 23:59", "2018-01-01 00:00")]

        // Skip month if day of month is specified and month has less days.

        [DataRow("0 0 0 30 * *", "2017-02-25 00:00", "2017-03-30 00:00")]
        [DataRow("0 0 0 31 * *", "2017-02-25 00:00", "2017-03-31 00:00")]
        [DataRow("0 0 0 31 * *", "2017-04-01 00:00", "2017-05-31 00:00")]

        // Leap year.

        [DataRow("0 0 0 29 2 *", "2016-03-10 00:00", "2020-02-29 00:00")]

        // Support 'L' character in day of month field.

        [DataRow("* * * L * *", "2016-01-05", "2016-01-31")]
        [DataRow("* * * L * *", "2016-01-31", "2016-01-31")]
        [DataRow("* * * L * *", "2016-02-05", "2016-02-29")]
        [DataRow("* * * L * *", "2016-02-29", "2016-02-29")]
        [DataRow("* * * L 2 *", "2016-02-29", "2016-02-29")]
        [DataRow("* * * L * *", "2017-02-28", "2017-02-28")]
        [DataRow("* * * L * *", "2016-03-05", "2016-03-31")]
        [DataRow("* * * L * *", "2016-03-31", "2016-03-31")]
        [DataRow("* * * L * *", "2016-04-05", "2016-04-30")]
        [DataRow("* * * L * *", "2016-04-30", "2016-04-30")]
        [DataRow("* * * L * *", "2016-05-05", "2016-05-31")]
        [DataRow("* * * L * *", "2016-05-31", "2016-05-31")]
        [DataRow("* * * L * *", "2016-06-05", "2016-06-30")]
        [DataRow("* * * L * *", "2016-06-30", "2016-06-30")]
        [DataRow("* * * L * *", "2016-07-05", "2016-07-31")]
        [DataRow("* * * L * *", "2016-07-31", "2016-07-31")]
        [DataRow("* * * L * *", "2016-08-05", "2016-08-31")]
        [DataRow("* * * L * *", "2016-08-31", "2016-08-31")]
        [DataRow("* * * L * *", "2016-09-05", "2016-09-30")]
        [DataRow("* * * L * *", "2016-09-30", "2016-09-30")]
        [DataRow("* * * L * *", "2016-10-05", "2016-10-31")]
        [DataRow("* * * L * *", "2016-10-31", "2016-10-31")]
        [DataRow("* * * L * *", "2016-11-05", "2016-11-30")]
        [DataRow("* * * L * *", "2016-12-05", "2016-12-31")]
        [DataRow("* * * L * *", "2016-12-31", "2016-12-31")]
        [DataRow("* * * L * *", "2099-12-05", "2099-12-31")]
        [DataRow("* * * L * *", "2099-12-31", "2099-12-31")]

        [DataRow("* * * L-1 * *", "2016-01-01", "2016-01-30")]
        [DataRow("* * * L-1 * *", "2016-01-29", "2016-01-30")]
        [DataRow("* * * L-1 * *", "2016-01-30", "2016-01-30")]
        [DataRow("* * * L-1 * *", "2016-01-31", "2016-02-28")]
        [DataRow("* * * L-1 * *", "2016-02-01", "2016-02-28")]
        [DataRow("* * * L-1 * *", "2016-02-28", "2016-02-28")]
        [DataRow("* * * L-1 * *", "2017-02-01", "2017-02-27")]
        [DataRow("* * * L-1 * *", "2017-02-27", "2017-02-27")]
        [DataRow("* * * L-1 * *", "2016-04-01", "2016-04-29")]
        [DataRow("* * * L-1 * *", "2016-04-29", "2016-04-29")]
        [DataRow("* * * L-1 * *", "2016-12-01", "2016-12-30")]

        [DataRow("* * * L-2 * *", "2016-01-05", "2016-01-29")]
        [DataRow("* * * L-2 * *", "2016-01-30", "2016-02-27")]
        [DataRow("* * * L-2 * *", "2016-02-01", "2016-02-27")]
        [DataRow("* * * L-2 * *", "2017-02-01", "2017-02-26")]
        [DataRow("* * * L-2 * *", "2016-04-01", "2016-04-28")]
        [DataRow("* * * L-2 * *", "2016-12-01", "2016-12-29")]
        [DataRow("* * * L-2 * *", "2016-12-29", "2016-12-29")]
        [DataRow("* * * L-2 * *", "2016-12-30", "2017-01-29")]

        [DataRow("* * * L-28 * *", "2016-01-01", "2016-01-03")]
        [DataRow("* * * L-28 * *", "2016-04-01", "2016-04-02")]
        [DataRow("* * * L-28 * *", "2016-02-01", "2016-02-01")]
        [DataRow("* * * L-28 * *", "2017-02-01", "2017-03-03")]

        [DataRow("* * * L-29 * *", "2016-01-01", "2016-01-02")]
        [DataRow("* * * L-29 * *", "2016-04-01", "2016-04-01")]
        [DataRow("* * * L-29 * *", "2016-02-01", "2016-03-02")]
        [DataRow("* * * L-29 * *", "2017-02-01", "2017-03-02")]

        [DataRow("* * * L-30 * *", "2016-01-01", "2016-01-01")]
        [DataRow("* * * L-30 * *", "2016-04-01", "2016-05-01")]
        [DataRow("* * * L-30 * *", "2016-02-01", "2016-03-01")]
        [DataRow("* * * L-30 * *", "2017-02-01", "2017-03-01")]

        // Support 'L' character in day of week field.

        // Monday        Tuesday       Wednesday     Thursday      Friday        Saturday      Sunday
        // 2016-01-23    2016-01-24    2016-01-25    2016-01-26    2016-01-27    2016-01-28    2016-01-29
        // 2016-01-30    2016-01-31

        // ReSharper disable StringLiteralTypo
        [DataRow("* * * * * 0L  ", "2017-01-29", "2017-01-29")]
        [DataRow("* * * * * 0L  ", "2017-01-01", "2017-01-29")]
        [DataRow("* * * * * SUNL", "2017-01-01", "2017-01-29")]
        [DataRow("* * * * * 1L  ", "2017-01-30", "2017-01-30")]
        [DataRow("* * * * * 1L  ", "2017-01-01", "2017-01-30")]
        [DataRow("* * * * * MONL", "2017-01-01", "2017-01-30")]
        [DataRow("* * * * * 2L  ", "2017-01-31", "2017-01-31")]
        [DataRow("* * * * * 2L  ", "2017-01-01", "2017-01-31")]
        [DataRow("* * * * * TUEL", "2017-01-01", "2017-01-31")]
        [DataRow("* * * * * 3L  ", "2017-01-25", "2017-01-25")]
        [DataRow("* * * * * 3L  ", "2017-01-01", "2017-01-25")]
        [DataRow("* * * * * WEDL", "2017-01-01", "2017-01-25")]
        [DataRow("* * * * * 4L  ", "2017-01-26", "2017-01-26")]
        [DataRow("* * * * * 4L  ", "2017-01-01", "2017-01-26")]
        [DataRow("* * * * * THUL", "2017-01-01", "2017-01-26")]
        [DataRow("* * * * * 5L  ", "2017-01-27", "2017-01-27")]
        [DataRow("* * * * * 5L  ", "2017-01-01", "2017-01-27")]
        [DataRow("* * * * * FRIL", "2017-01-01", "2017-01-27")]
        [DataRow("* * * * * 6L  ", "2017-01-28", "2017-01-28")]
        [DataRow("* * * * * 6L  ", "2017-01-01", "2017-01-28")]
        [DataRow("* * * * * SATL", "2017-01-01", "2017-01-28")]
        [DataRow("* * * * * 7L  ", "2017-01-29", "2017-01-29")]
        [DataRow("* * * * * 7L  ", "2016-12-31", "2017-01-29")]
        // ReSharper restore StringLiteralTypo

        // Support '#' in day of week field.

        [DataRow("* * * * * SUN#1", "2017-01-01", "2017-01-01")]
        [DataRow("* * * * * 0#1  ", "2017-01-01", "2017-01-01")]
        [DataRow("* * * * * 0#1  ", "2016-12-10", "2017-01-01")]
        [DataRow("* * * * * 0#1  ", "2017-02-01", "2017-02-05")]
        [DataRow("* * * * * 0#2  ", "2017-01-01", "2017-01-08")]
        [DataRow("* * * * * 0#2  ", "2017-01-08", "2017-01-08")]
        [DataRow("* * * * * 5#3  ", "2017-01-01", "2017-01-20")]
        [DataRow("* * * * * 5#3  ", "2017-01-21", "2017-02-17")]
        [DataRow("* * * * * 3#2  ", "2017-01-01", "2017-01-11")]
        [DataRow("* * * * * 2#5  ", "2017-02-01", "2017-05-30")]

        // Support 'W' in day of month field.

        [DataRow("* * * 1W * *", "2017-01-01", "2017-01-02")]
        [DataRow("* * * 2W * *", "2017-01-02", "2017-01-02")]
        [DataRow("* * * 6W * *", "2017-01-02", "2017-01-06")]
        [DataRow("* * * 7W * *", "2017-01-02", "2017-01-06")]
        [DataRow("* * * 7W * *", "2017-01-07", "2017-02-07")]
        [DataRow("* * * 8W * *", "2017-01-02", "2017-01-09")]

        [DataRow("* * * 30W * *", "2017-04-27", "2017-04-28")]
        [DataRow("* * * 30W * *", "2017-04-28", "2017-04-28")]
        [DataRow("* * * 30W * *", "2017-04-29", "2017-05-30")]

        [DataRow("* * * 1W * *", "2017-04-01", "2017-04-03")]

        [DataRow("0 30    10 1W * *", "2017-04-01 00:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 1W * *", "2017-04-01 12:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 1W * *", "2017-04-02 00:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 1W * *", "2017-04-02 12:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 1W * *", "2017-04-03 00:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 1W * *", "2017-04-03 12:00", "2017-05-01 10:30")]

        [DataRow("0 30    10 2W * *", "2017-04-01 00:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 2W * *", "2017-04-01 12:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 2W * *", "2017-04-02 00:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 2W * *", "2017-04-02 12:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 2W * *", "2017-04-03 00:00", "2017-04-03 10:30")]
        [DataRow("0 30    10 2W * *", "2017-04-03 12:00", "2017-05-02 10:30")]

        [DataRow("0 30    17 7W * *", "2017-01-06 17:45", "2017-02-07 17:30")]
        [DataRow("0 30,45 17 7W * *", "2017-01-06 17:45", "2017-01-06 17:45")]
        [DataRow("0 30,55 17 7W * *", "2017-01-06 17:45", "2017-01-06 17:55")]

        [DataRow("0 30    17 8W * *", "2017-01-08 19:45", "2017-01-09 17:30")]

        [DataRow("0 30    17 30W * *", "2017-04-28 17:45", "2017-05-30 17:30")]
        [DataRow("0 30,45 17 30W * *", "2017-04-28 17:45", "2017-04-28 17:45")]
        [DataRow("0 30,55 17 30W * *", "2017-04-28 17:45", "2017-04-28 17:55")]

        [DataRow("0 30    17 30W * *", "2017-02-06 00:00", "2017-03-30 17:30")]

        [DataRow("0 30    17 31W * *", "2018-03-30 17:45", "2018-05-31 17:30")]
        [DataRow("0 30    17 15W * *", "2016-12-30 17:45", "2017-01-16 17:30")]

        [DataRow("0 30    17 27W * 1L ", "2017-03-10 17:45", "2017-03-27 17:30")]
        [DataRow("0 30    17 27W * 1#4", "2017-03-10 17:45", "2017-03-27 17:30")]

        // Support 'LW' in day of month field.

        [DataRow("* * * LW * *", "2017-01-01", "2017-01-31")]
        [DataRow("* * * LW * *", "2017-09-01", "2017-09-29")]
        [DataRow("* * * LW * *", "2017-09-29", "2017-09-29")]
        [DataRow("* * * LW * *", "2017-09-30", "2017-10-31")]
        [DataRow("* * * LW * *", "2017-04-01", "2017-04-28")]
        [DataRow("* * * LW * *", "2017-04-28", "2017-04-28")]
        [DataRow("* * * LW * *", "2017-04-29", "2017-05-31")]
        [DataRow("* * * LW * *", "2017-05-30", "2017-05-31")]

        [DataRow("0 30 17 LW * *", "2017-09-29 17:45", "2017-10-31 17:30")]

        [DataRow("* * * L-1W * *", "2017-01-01", "2017-01-30")]
        [DataRow("* * * L-2W * *", "2017-01-01", "2017-01-30")]
        [DataRow("* * * L-3W * *", "2017-01-01", "2017-01-27")]
        [DataRow("* * * L-4W * *", "2017-01-01", "2017-01-27")]

        [DataRow("* * * L-0W * *", "2016-02-01", "2016-02-29")]
        [DataRow("* * * L-0W * *", "2017-02-01", "2017-02-28")]
        [DataRow("* * * L-1W * *", "2016-02-01", "2016-02-29")]
        [DataRow("* * * L-1W * *", "2017-02-01", "2017-02-27")]
        [DataRow("* * * L-2W * *", "2016-02-01", "2016-02-26")]
        [DataRow("* * * L-2W * *", "2017-02-01", "2017-02-27")]
        [DataRow("* * * L-3W * *", "2016-02-01", "2016-02-26")]
        [DataRow("* * * L-3W * *", "2017-02-01", "2017-02-24")]

        // Support '?'.

        [DataRow("* * * ? 11 *", "2016-10-09", "2016-11-01")]

        [DataRow("? ? ? ? ? ?", "2016-12-09 16:46", "2016-12-09 16:46")]
        [DataRow("* * * * * ?", "2016-12-09 16:46", "2016-12-09 16:46")]
        [DataRow("* * * ? * *", "2016-03-09 16:46", "2016-03-09 16:46")]
        [DataRow("* * * * * ?", "2016-12-30 16:46", "2016-12-30 16:46")]
        [DataRow("* * * ? * *", "2016-12-09 02:46", "2016-12-09 02:46")]
        [DataRow("* * * * * ?", "2016-12-09 16:09", "2016-12-09 16:09")]
        [DataRow("* * * ? * *", "2099-12-09 16:46", "2099-12-09 16:46")]

        // Day of 100-year and not 400-year.
        [DataRow("* * * * * *", "1900-02-20 16:46", "1900-02-20 16:46")]

        // Day of 400-year
        [DataRow("* * * * * *", "2000-02-28 16:46", "2000-02-28 16:46")]

        // Last day of 400-year.
        [DataRow("* * * * * *", "2000-12-31 16:46", "2000-12-31 16:46")]

        // Case insensitive.
        [DataRow("* *  *  lw   * *   ", "2017-05-30", "2017-05-31")]
        [DataRow("* *  *  l-0w * *   ", "2016-02-01", "2016-02-29")]
        [DataRow("0 30 17 27w  * 1l  ", "2017-03-10 17:45", "2017-03-27 17:30")]
        [DataRow("0 30 17 27w  * mOnL", "2017-03-10 17:45", "2017-03-27 17:30")]

        // Complex expressions
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:00", "2017-04-17 17:20")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:21", "2017-04-17 17:30")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:31", "2017-04-17 17:32")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:33", "2017-04-17 17:34")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:35", "2017-04-17 17:40")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:41", "2017-04-17 17:50")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:51", "2017-04-17 17:57")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:58", "2017-04-17 17:58")]
        [DataRow("0 57,20/20,30/20,32-34/2,58 * * * * ", "2017-04-17 17:59", "2017-04-17 18:20")]
        public void GetNextOccurrence_ReturnsCorrectDate(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            var fromInstant = GetInstantFromLocalTime(fromString, EasternTimeZone);
            var occurrence = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive: true);
            Assert.AreEqual(GetInstantFromLocalTime(expectedString, EasternTimeZone), occurrence);
        }
        #endregion


        [TestMethod]
        [DataRow(true, 00001)]
        [DataRow(true, 09999)]
        [DataRow(false, 0001)]
        [DataRow(false, 9999)]
        public void GetNextOccurrence_RoundsFromUtcUpToTheSecond(bool inclusiveFrom, int extraTicks)
        {
            var expression = CronExpression.Parse("* * * * * *", CronFormat.IncludeSeconds);
            var fromUtc = new DateTime(2017, 07, 20, 11, 59, 59, DateTimeKind.Utc).AddTicks(extraTicks);

            var occurrence = expression.GetNextOccurrence(fromUtc, inclusive: inclusiveFrom);

            Assert.AreEqual(new DateTime(2017, 07, 20, 12, 0, 0, DateTimeKind.Utc), occurrence);
        }

        [TestMethod]

        // 2016-03-13 is date when the clock jumps forward from 1:59 am -05:00 standard time (ST) to 3:00 am -04:00 DST in Eastern Time Zone.
        // ________1:59 ST///invalid///3:00 DST________

        // Run missed.

        [DataRow("0 */30 *      *  *  *    ", "2016-03-13 01:45 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 */30 */2    *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 1-58 */2    *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 0,30 0-23/2 *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 */30 2      *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 0,30 2      *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 */30 2      13 03 *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 0,30 02     13 03 *    ", "2016-03-13 01:45 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 30   2      *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 0    */2    *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]
        [DataRow("0 30   0-23/2 *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]

        [DataRow("0 0,59 *      *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 01:59 -05:00", true)]
        [DataRow("0 0,59 *      *  *  *    ", "2016-03-13 03:00 -04:00", "2016-03-13 03:00 -04:00", true)]

        [DataRow("0 30   *      *  3  SUN#2", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", true)]

        [DataRow("0 */30 *      *  *  *    ", "2016-03-13 01:30 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 */30 */2    *  *  *    ", "2016-03-13 01:30 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 1-58 */2    *  *  *    ", "2016-03-13 01:58 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 0,30 0-23/2 *  *  *    ", "2016-03-13 00:30 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 0,30 2      *  *  *    ", "2016-03-12 02:30 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 */30 2      13 03 *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 0,30 02     13 03 *    ", "2016-03-13 01:45 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 30   2      *  *  *    ", "2016-03-12 02:30 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 0    */2    *  *  *    ", "2016-03-13 00:00 -05:00", "2016-03-13 03:00 -04:00", false)]
        [DataRow("0 30   0-23/2 *  *  *    ", "2016-03-13 00:30 -05:00", "2016-03-13 03:00 -04:00", false)]

        [DataRow("0 0,59 *      *  *  *    ", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", false)]

        [DataRow("0 30   *      *  3  SUN#2", "2016-03-13 01:59 -05:00", "2016-03-13 03:00 -04:00", false)]
        public void GetNextOccurrence_HandleDST_WhenTheClockJumpsForward_And_TimeZoneIsEst(string cronExpression, string fromString, string expectedString, bool inclusive)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive);

            Assert.AreEqual(expectedInstant, executed);
            Assert.AreEqual(expectedInstant.Offset, executed?.Offset);
        }

        [TestMethod]

        // 2017-03-31 00:00 is time in Jordan Time Zone when the clock jumps forward
        // from 2017-03-30 23:59:59.9999999 +02:00 standard time (ST) to 01:00:00.0000000 am +03:00 DST.
        // ________23:59:59.9999999 ST///invalid///01:00:00.0000000 DST________

        // Run missed.

        [DataRow("30 0 L  * *", "2017-03-30 23:59:59.9999999 +02:00", "2017-03-31 01:00:00 +03:00", false)]
        [DataRow("30 0 L  * *", "2017-03-30 23:59:59.9999000 +02:00", "2017-03-31 01:00:00 +03:00", false)]
        [DataRow("30 0 L  * *", "2017-03-30 23:59:59.9990000 +02:00", "2017-03-31 01:00:00 +03:00", false)]
        [DataRow("30 0 L  * *", "2017-03-30 23:59:59.9900000 +02:00", "2017-03-31 01:00:00 +03:00", false)]
        [DataRow("30 0 L  * *", "2017-03-30 23:59:59.9000000 +02:00", "2017-03-31 01:00:00 +03:00", false)]
        [DataRow("30 0 L  * *", "2017-03-30 23:59:59.0000000 +02:00", "2017-03-31 01:00:00 +03:00", false)]

        [DataRow("30 0 L  * *", "2017-03-31 01:00:00.0000001 +02:00", "2017-04-30 00:30:00 +03:00", true)]
        public void GetNextOccurrence_HandleDST_WhenTheClockJumpsForwardAndFromIsAroundDST(string cronExpression, string fromString, string expectedString, bool inclusive)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, JordanTimeZone, inclusive);

            Assert.AreEqual(expectedInstant, executed);
            Assert.AreEqual(expectedInstant.Offset, executed?.Offset);
        }

        [TestMethod]

        // 2017-05-14 00:00 is time in Chile Time Zone when the clock jumps backward
        // from 2017-05-13 23:59:59.9999999 -03:00 standard time (ST) to 2017-05-13 23:00:00.0000000 am -04:00 DST .
        // ________23:59:59.9999999 -03:00 ST -> 23:00:00.0000000 -04:00 DST

        [DataRow("30 23 * * *", "2017-05-13 23:59:59.9999999 -03:00", "2017-05-14 23:30:00 -04:00", false)]
        [DataRow("30 23 * * *", "2017-05-13 23:59:59.9999000 -03:00", "2017-05-14 23:30:00 -04:00", false)]
        [DataRow("30 23 * * *", "2017-05-13 23:59:59.9990000 -03:00", "2017-05-14 23:30:00 -04:00", false)]
        [DataRow("30 23 * * *", "2017-05-13 23:59:59.9900000 -03:00", "2017-05-14 23:30:00 -04:00", false)]
        [DataRow("30 23 * * *", "2017-05-13 23:59:59.9000000 -03:00", "2017-05-14 23:30:00 -04:00", false)]
        [DataRow("30 23 * * *", "2017-05-13 23:59:59.0000000 -03:00", "2017-05-14 23:30:00 -04:00", false)]

        [DataRow("30 23 * * *", "2017-05-14 00:00:00.0000001 -04:00", "2017-05-14 23:30:00 -04:00", true)]
        public void GetNextOccurrence_HandleDST_WhenTheClockJumpsBackwardAndFromIsAroundDST(string cronExpression, string fromString, string expectedString, bool inclusive)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, PacificTimeZone, inclusive);

            Assert.AreEqual(expectedInstant, executed);
            Assert.AreEqual(expectedInstant.Offset, executed?.Offset);
        }

        [TestMethod]
        [DataRow("0  7 * * *", "2020-05-20 07:00:00.0000001 -04:00", "2020-05-21 07:00:00 -04:00", true)]
        [DataRow("0  7 * * *", "2020-05-20 07:00:00.0000001 -04:00", "2020-05-21 07:00:00 -04:00", false)]

        [DataRow("0  7 * * *", "2023-08-12 07:00:00.9999999 -04:00", "2023-08-13 07:00:00 -04:00", true)]
        [DataRow("0  7 * * *", "2023-08-12 07:00:00.9999999 -04:00", "2023-08-13 07:00:00 -04:00", false)]
        public void GetNextOccurrence_ReturnsCorrectDate_WhenFromIsNotRoundAndZoneIsSpecified(string cronExpression, string fromString, string expectedString, bool inclusive)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive);

            Assert.AreEqual(expectedInstant, executed);
            Assert.AreEqual(expectedInstant.Offset, executed?.Offset);
        }

        [TestMethod]

        // 2017-10-01 is date when the clock jumps forward from 1:59 am +10:30 standard time (ST) to 2:30 am +11:00 DST on Lord Howe.
        // ________1:59 ST///invalid///2:30 DST________

        // Run missed.

        [DataRow("0 */30 *      *  *  *    ", "2017-10-01 01:45 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 */30 */2    *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 1-58 */2    *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 0,30 0-23/2 *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 */30 2      *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 0,30 2      *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 */30 2      01 10 *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 0,30 02     01 10 *    ", "2017-10-01 01:45 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 30   2      *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 0,30 */2    *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        [DataRow("0 30   0-23/2 *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]

        [DataRow("0 0,30,59 *      *  *  *    ", "2017-10-01 01:59 +10:30", "2017-10-01 01:59 +10:30")]
        [DataRow("0 0,30,59 *      *  *  *    ", "2017-10-01 02:30 +11:00", "2017-10-01 02:30 +11:00")]

        [DataRow("0 30   *      *  10 SUN#1", "2017-10-01 01:59 +10:30", "2017-10-01 02:30 +11:00")]
        public void GetNextOccurrence_HandleDST_WhenTheClockTurnForwardHalfHour(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, LordHoweTimeZone, inclusive: true);

            Assert.AreEqual(expectedInstant, executed);
            Assert.AreEqual(expectedInstant.Offset, executed?.Offset);
        }

        [TestMethod]

        // 2016-11-06 is date when the clock jumps backward from 2:00 am -04:00 DST to 1:00 am -05:00 ST in Eastern Time Zone.
        // _______1:00 DST____1:59 DST -> 1:00 ST____2:00 ST_______

        // Run at 2:00 ST because 2:00 DST is invalid.
        [DataRow("0 */30 */2 * * *", "2016-11-06 01:30 -04:00", "2016-11-06 02:00 -05:00", true)]
        [DataRow("0 0    */2 * * *", "2016-11-06 00:30 -04:00", "2016-11-06 02:00 -05:00", true)]
        [DataRow("0 0    0/2 * * *", "2016-11-06 00:30 -04:00", "2016-11-06 02:00 -05:00", true)]
        [DataRow("0 0    2-3 * * *", "2016-11-06 00:30 -04:00", "2016-11-06 02:00 -05:00", true)]

        // Run twice due to intervals.
        [DataRow("0 */30 *   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:00 -05:00", true)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:15 -05:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:30 -05:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:45 -05:00", "2016-11-06 02:00 -05:00", true)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:30 -04:00", false)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:00 -05:00", false)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:00 -05:00", "2016-11-06 01:30 -05:00", false)]
        [DataRow("0 */30 *   * * *", "2016-11-06 01:30 -05:00", "2016-11-06 02:00 -05:00", false)]

        [DataRow("0 30   *   * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 30   *   * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 30   *   * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -05:00", false)]
        [DataRow("0 30   *   * * *", "2016-11-06 01:30 -05:00", "2016-11-06 02:30 -05:00", false)]

        [DataRow("0 30   */1  * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 30   */1  * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 30   0/1  * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 30   0/1  * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 30   */1  * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -05:00", false)]
        [DataRow("0 30   0/1  * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -05:00", false)]

        [DataRow("0 30   1-9 * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 30   1-9 * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 30   1-9 * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:30 -05:00", false)]

        [DataRow("0 */30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 */30 1   * * *", "2016-11-06 01:20 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 */30 1   * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:00 -05:00", true)]
        [DataRow("0 */30 1   * * *", "2016-11-06 01:20 -05:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 */30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:30 -04:00", false)]
        [DataRow("0 */30 1   * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:00 -05:00", false)]

        [DataRow("0 0/30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0/30 1   * * *", "2016-11-06 01:20 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 0/30 1   * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:00 -05:00", true)]
        [DataRow("0 0/30 1   * * *", "2016-11-06 01:20 -05:00", "2016-11-06 01:30 -05:00", true)]
        [DataRow("0 0/30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:30 -04:00", false)]
        [DataRow("0 0/30 1   * * *", "2016-11-06 01:30 -04:00", "2016-11-06 01:00 -05:00", false)]
        [DataRow("0 0/30 1   * * *", "2016-11-06 01:00 -05:00", "2016-11-06 01:30 -05:00", false)]

        [DataRow("0 0-30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0-30 1   * * *", "2016-11-06 01:20 -04:00", "2016-11-06 01:20 -04:00", true)]
        [DataRow("0 0-30 1   * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:00 -05:00", true)]
        [DataRow("0 0-30 1   * * *", "2016-11-06 01:20 -05:00", "2016-11-06 01:20 -05:00", true)]
        [DataRow("0 0-30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:01 -04:00", false)]
        [DataRow("0 0-30 1   * * *", "2016-11-06 01:20 -04:00", "2016-11-06 01:21 -04:00", false)]
        [DataRow("0 0-30 1   * * *", "2016-11-06 01:59 -04:00", "2016-11-06 01:00 -05:00", false)]
        [DataRow("0 0-30 1   * * *", "2016-11-06 01:20 -05:00", "2016-11-06 01:21 -05:00", false)]

        [DataRow("*/30 0 1 * * *", "2016-11-06 00:30:00 -04:00", "2016-11-06 01:00:00 -04:00", true)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:01 -04:00", "2016-11-06 01:00:30 -04:00", true)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:31 -04:00", "2016-11-06 01:00:00 -05:00", true)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:01 -05:00", "2016-11-06 01:00:30 -05:00", true)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:31 -05:00", "2016-11-07 01:00:00 -05:00", true)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 00:30:00 -04:00", "2016-11-06 01:00:00 -04:00", false)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:00 -04:00", "2016-11-06 01:00:30 -04:00", false)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:30 -04:00", "2016-11-06 01:00:00 -05:00", false)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:00 -05:00", "2016-11-06 01:00:30 -05:00", false)]
        [DataRow("*/30 0 1 * * *", "2016-11-06 01:00:30 -05:00", "2016-11-07 01:00:00 -05:00", false)]

        [DataRow("0/30 0 1 * * *", "2016-11-06 00:30:00 -04:00", "2016-11-06 01:00:00 -04:00", true)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:01 -04:00", "2016-11-06 01:00:30 -04:00", true)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:31 -04:00", "2016-11-06 01:00:00 -05:00", true)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:01 -05:00", "2016-11-06 01:00:30 -05:00", true)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:31 -05:00", "2016-11-07 01:00:00 -05:00", true)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 00:30:00 -04:00", "2016-11-06 01:00:00 -04:00", false)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:00 -04:00", "2016-11-06 01:00:30 -04:00", false)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:30 -04:00", "2016-11-06 01:00:00 -05:00", false)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:00 -05:00", "2016-11-06 01:00:30 -05:00", false)]
        [DataRow("0/30 0 1 * * *", "2016-11-06 01:00:30 -05:00", "2016-11-07 01:00:00 -05:00", false)]

        [DataRow("0-30 0 1 * * *", "2016-11-06 00:30:00 -04:00", "2016-11-06 01:00:00 -04:00", true)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:01 -04:00", "2016-11-06 01:00:01 -04:00", true)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:31 -04:00", "2016-11-06 01:00:00 -05:00", true)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:01 -05:00", "2016-11-06 01:00:01 -05:00", true)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:31 -05:00", "2016-11-07 01:00:00 -05:00", true)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 00:30:00 -04:00", "2016-11-06 01:00:00 -04:00", false)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:00 -04:00", "2016-11-06 01:00:01 -04:00", false)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:30 -04:00", "2016-11-06 01:00:00 -05:00", false)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:00 -05:00", "2016-11-06 01:00:01 -05:00", false)]
        [DataRow("0-30 0 1 * * *", "2016-11-06 01:00:30 -05:00", "2016-11-07 01:00:00 -05:00", false)]

        // Duplicates skipped due to certain time.
        [DataRow("0 0,30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0,30 1   * * *", "2016-11-06 01:20 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 0,30 1   * * *", "2016-11-06 01:00 -05:00", "2016-11-07 01:00 -05:00", true)]
        [DataRow("0 0,30 1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:30 -04:00", false)]
        [DataRow("0 0,30 1   * * *", "2016-11-06 01:30 -04:00", "2016-11-07 01:00 -05:00", false)]

        [DataRow("0 0,30 1   * 1/2 *", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0,30 1   * 1/2 *", "2016-11-06 01:20 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 0,30 1   * 1/2 *", "2016-11-06 01:00 -05:00", "2016-11-07 01:00 -05:00", true)]
        [DataRow("0 0,30 1   * 1/2 *", "2016-11-06 01:00 -04:00", "2016-11-06 01:30 -04:00", false)]
        [DataRow("0 0,30 1   * 1/2 *", "2016-11-06 01:30 -04:00", "2016-11-07 01:00 -05:00", false)]

        [DataRow("0 0,30 1   6/1 1-12 0/1", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0,30 1   6/1 1-12 0/1", "2016-11-06 01:20 -04:00", "2016-11-06 01:30 -04:00", true)]
        [DataRow("0 0,30 1   6/1 1-12 0/1", "2016-11-06 01:00 -05:00", "2016-11-07 01:00 -05:00", true)]
        [DataRow("0 0,30 1   6/1 1-12 0/1", "2016-11-06 01:00 -04:00", "2016-11-06 01:30 -04:00", false)]
        [DataRow("0 0,30 1   6/1 1-12 0/1", "2016-11-06 01:30 -04:00", "2016-11-07 01:00 -05:00", false)]

        [DataRow("0 0    1   * * *", "2016-11-06 01:00 -04:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0    1   * * *", "2016-11-06 01:00 -05:00", "2016-11-07 01:00 -05:00", true)]
        [DataRow("0 0    1   * * *", "2016-11-06 01:00 -04:00", "2016-11-07 01:00 -05:00", false)]

        [DataRow("0 0    1   6 11 *", "2015-11-07 01:00 -05:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0    1   6 11 *", "2015-11-07 01:00 -05:00", "2016-11-06 01:00 -04:00", false)]

        [DataRow("0 0    1   * 11 SUN#1", "2015-11-01 01:00 -05:00", "2016-11-06 01:00 -04:00", true)]
        [DataRow("0 0    1   * 11 SUN#1", "2015-11-01 01:00 -05:00", "2016-11-06 01:00 -04:00", false)]

        // Run at 02:00 ST because 02:00 doesn't exist in DST.

        [DataRow("0 0 2 * * *", "2016-11-06 01:45 -04:00", "2016-11-06 02:00 -05:00", false)]
        [DataRow("0 0 2 * * *", "2016-11-06 01:45 -05:00", "2016-11-06 02:00 -05:00", false)]
        public void GetNextOccurrence_HandleDST_WhenTheClockJumpsBackward(string cronExpression, string fromString, string expectedString, bool inclusive)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive);

            Assert.AreEqual(expectedInstant, executed);
            Assert.AreEqual(expectedInstant.Offset, executed?.Offset);
        }

        [TestMethod]
        public void GetNextOccurrence_HandlesBorderConditions_WhenDSTEnds()
        {
            var expression = CronExpression.Parse("59 59 01 * * *", CronFormat.IncludeSeconds);

            var from = new DateTimeOffset(2016, 11, 06, 02, 00, 00, 00, TimeSpan.FromHours(-5)).AddTicks(-1);

            var executed = expression.GetNextOccurrence(from, EasternTimeZone, inclusive: true);

            Assert.AreEqual(new DateTimeOffset(2016, 11, 07, 01, 59, 59, 00, TimeSpan.FromHours(-5)), executed);
            Assert.AreEqual(TimeSpan.FromHours(-5), executed?.Offset);
        }

        [TestMethod]

        // 2017-04-02 is date when the clock jumps backward from 2:00 am -+11:00 DST to 1:30 am +10:30 ST on Lord Howe.
        // _______1:30 DST____1:59 DST -> 1:30 ST____2:00 ST_______

        // Run at 2:00 ST because 2:00 DST is invalid.
        [DataRow("0 */30 */2 * * *", "2017-04-02 01:30 +11:00", "2017-04-02 02:00 +10:30")]
        [DataRow("0 0    */2 * * *", "2017-04-02 00:30 +11:00", "2017-04-02 02:00 +10:30")]
        [DataRow("0 0    0/2 * * *", "2017-04-02 00:30 +11:00", "2017-04-02 02:00 +10:30")]
        [DataRow("0 0    2-3 * * *", "2017-04-02 00:30 +11:00", "2017-04-02 02:00 +10:30")]

        // Run twice due to intervals.
        [DataRow("0 */30 *   * * *", "2017-04-02 01:30 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 */30 *   * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]
        [DataRow("0 */30 *   * * *", "2017-04-02 01:15 +10:30", "2017-04-02 01:30 +10:30")]

        [DataRow("0 30   *   * * *", "2017-04-02 01:30 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 30   *   * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]

        [DataRow("0 30   */1 * * *", "2017-04-02 01:30 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 30   */1 * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]
        [DataRow("0 30   0/1 * * *", "2017-04-02 01:30 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 30   0/1 * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]

        [DataRow("0 30   1-9 * * *", "2017-04-02 01:30 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 30   1-9 * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]

        [DataRow("0 */30 1   * * *", "2017-04-02 01:00 +11:00", "2017-04-02 01:00 +11:00")]
        [DataRow("0 */30 1   * * *", "2017-04-02 01:20 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 */30 1   * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]

        [DataRow("0 0/30 1   * * *", "2017-04-02 01:00 +11:00", "2017-04-02 01:00 +11:00")]
        [DataRow("0 0/30 1   * * *", "2017-04-02 01:20 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 0/30 1   * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]

        [DataRow("0 0-30 1   * * *", "2017-04-02 01:00 +11:00", "2017-04-02 01:00 +11:00")]
        [DataRow("0 0-30 1   * * *", "2017-04-02 01:20 +11:00", "2017-04-02 01:20 +11:00")]
        [DataRow("0 0-30 1   * * *", "2017-04-02 01:59 +11:00", "2017-04-02 01:30 +10:30")]

        [DataRow("*/30 30 1 * * *", "2017-04-02 00:30:00 +11:00", "2017-04-02 01:30:00 +11:00")]
        [DataRow("*/30 30 1 * * *", "2017-04-02 01:30:01 +11:00", "2017-04-02 01:30:30 +11:00")]
        [DataRow("*/30 30 1 * * *", "2017-04-02 01:30:31 +11:00", "2017-04-02 01:30:00 +10:30")]
        [DataRow("*/30 30 1 * * *", "2017-04-02 01:30:01 +10:30", "2017-04-02 01:30:30 +10:30")]
        [DataRow("*/30 30 1 * * *", "2017-04-02 01:30:31 +10:30", "2017-04-03 01:30:00 +10:30")]

        [DataRow("0/30 30 1 * * *", "2017-04-02 00:30:00 +11:00", "2017-04-02 01:30:00 +11:00")]
        [DataRow("0/30 30 1 * * *", "2017-04-02 01:30:01 +11:00", "2017-04-02 01:30:30 +11:00")]
        [DataRow("0/30 30 1 * * *", "2017-04-02 01:30:31 +11:00", "2017-04-02 01:30:00 +10:30")]
        [DataRow("0/30 30 1 * * *", "2017-04-02 01:30:01 +10:30", "2017-04-02 01:30:30 +10:30")]
        [DataRow("0/30 30 1 * * *", "2017-04-02 01:30:31 +10:30", "2017-04-03 01:30:00 +10:30")]

        [DataRow("0-30 30 1 * * *", "2017-04-02 00:30:00 +11:00", "2017-04-02 01:30:00 +11:00")]
        [DataRow("0-30 30 1 * * *", "2017-04-02 01:30:01 +11:00", "2017-04-02 01:30:01 +11:00")]
        [DataRow("0-30 30 1 * * *", "2017-04-02 01:30:31 +11:00", "2017-04-02 01:30:00 +10:30")]
        [DataRow("0-30 30 1 * * *", "2017-04-02 01:30:01 +10:30", "2017-04-02 01:30:01 +10:30")]
        [DataRow("0-30 30 1 * * *", "2017-04-02 01:30:31 +10:30", "2017-04-03 01:30:00 +10:30")]

        // Duplicates skipped due to certain time.
        [DataRow("0 0,30 1   * * *", "2017-04-02 01:00 +11:00", "2017-04-02 01:00 +11:00")]
        [DataRow("0 0,30 1   * * *", "2017-04-02 01:20 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 0,30 1   * * *", "2017-04-02 01:30 +10:30", "2017-04-03 01:00 +10:30")]

        [DataRow("0 0,30 1   * 2/2 *", "2017-04-02 01:00 +11:00", "2017-04-02 01:00 +11:00")]
        [DataRow("0 0,30 1   * 2/2 *", "2017-04-02 01:20 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 0,30 1   * 2/2 *", "2017-04-02 01:30 +10:30", "2017-04-03 01:00 +10:30")]

        [DataRow("0 0,30 1   2/1 1-12 0/1", "2017-04-02 01:00 +11:00", "2017-04-02 01:00 +11:00")]
        [DataRow("0 0,30 1   2/1 1-12 0/1", "2017-04-02 01:20 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 0,30 1   2/1 1-12 0/1", "2017-04-02 01:30 +10:30", "2017-04-03 01:00 +10:30")]

        [DataRow("0 30    1   * * *", "2017-04-02 01:30 +11:00", "2017-04-02 01:30 +11:00")]
        [DataRow("0 30    1   * * *", "2017-04-02 01:30 +10:30", "2017-04-03 01:30 +10:30")]
        public void GetNextOccurrence_HandleDST_WhenTheClockJumpsBackwardAndDeltaIsNotHour(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var executed = expression.GetNextOccurrence(fromInstant, LordHoweTimeZone, inclusive: true);

            Assert.AreEqual(expectedInstant, executed);
            Assert.AreEqual(expectedInstant.Offset, executed?.Offset);
        }

        [TestMethod]
        [DataRow("* * * * * *", "15:30", "15:30")]
        [DataRow("0 5 * * * *", "00:00", "00:05")]

        // Dst doesn't affect result.

        [DataRow("0 */30 * * * *", "2016-03-12 23:15", "2016-03-12 23:30")]
        [DataRow("0 */30 * * * *", "2016-03-12 23:45", "2016-03-13 00:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 00:15", "2016-03-13 00:30")]
        [DataRow("0 */30 * * * *", "2016-03-13 00:45", "2016-03-13 01:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 01:45", "2016-03-13 02:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 02:15", "2016-03-13 02:30")]
        [DataRow("0 */30 * * * *", "2016-03-13 02:45", "2016-03-13 03:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 03:15", "2016-03-13 03:30")]
        [DataRow("0 */30 * * * *", "2016-03-13 03:45", "2016-03-13 04:00")]

        [DataRow("0 */30 * * * *", "2016-11-05 23:10", "2016-11-05 23:30")]
        [DataRow("0 */30 * * * *", "2016-11-05 23:50", "2016-11-06 00:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 00:10", "2016-11-06 00:30")]
        [DataRow("0 */30 * * * *", "2016-11-06 00:50", "2016-11-06 01:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 01:10", "2016-11-06 01:30")]
        [DataRow("0 */30 * * * *", "2016-11-06 01:50", "2016-11-06 02:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 02:10", "2016-11-06 02:30")]
        [DataRow("0 */30 * * * *", "2016-11-06 02:50", "2016-11-06 03:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 03:10", "2016-11-06 03:30")]
        [DataRow("0 */30 * * * *", "2016-11-06 03:50", "2016-11-06 04:00")]
        public void GetNextOccurrence_ReturnsCorrectUtcDateTimeOffset(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstantFromLocalTime(fromString, TimeZoneInfo.Utc);
            var expectedInstant = GetInstantFromLocalTime(expectedString, TimeZoneInfo.Utc);

            var occurrence = expression.GetNextOccurrence(fromInstant, TimeZoneInfo.Utc, inclusive: true);

            Assert.AreEqual(expectedInstant, occurrence);
            Assert.AreEqual(expectedInstant.Offset, occurrence?.Offset);
        }

        [TestMethod]

        // Dst doesn't affect result.

        [DataRow("0 */30 * * * *", "2016-03-12 23:15 -05:00", "2016-03-12 23:30 -05:00")]
        [DataRow("0 */30 * * * *", "2016-03-12 23:45 -05:00", "2016-03-13 00:00 -05:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 00:15 -05:00", "2016-03-13 00:30 -05:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 00:45 -05:00", "2016-03-13 01:00 -05:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 01:45 -05:00", "2016-03-13 03:00 -04:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 03:15 -04:00", "2016-03-13 03:30 -04:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 03:45 -04:00", "2016-03-13 04:00 -04:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 04:15 -04:00", "2016-03-13 04:30 -04:00")]
        [DataRow("0 */30 * * * *", "2016-03-13 04:45 -04:00", "2016-03-13 05:00 -04:00")]

        [DataRow("0 */30 * * * *", "2016-11-05 23:10 -04:00", "2016-11-05 23:30 -04:00")]
        [DataRow("0 */30 * * * *", "2016-11-05 23:50 -04:00", "2016-11-06 00:00 -04:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 00:10 -04:00", "2016-11-06 00:30 -04:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 00:50 -04:00", "2016-11-06 01:00 -04:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 01:10 -04:00", "2016-11-06 01:30 -04:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 01:50 -04:00", "2016-11-06 01:00 -05:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 01:10 -05:00", "2016-11-06 01:30 -05:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 01:50 -05:00", "2016-11-06 02:00 -05:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 02:10 -05:00", "2016-11-06 02:30 -05:00")]
        [DataRow("0 */30 * * * *", "2016-11-06 02:50 -05:00", "2016-11-06 03:00 -05:00")]
        public void GetNextOccurrence_ReturnsCorrectDateTimeOffset(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var occurrence = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive: true);

            Assert.AreEqual(expectedInstant, occurrence);
            Assert.AreEqual(expectedInstant.Offset, occurrence?.Offset);
        }

        [TestMethod]
        [DataRow("* * * * 4 *", "2099-12-13 00:00:00")]
        public void GetNextOccurrence_ReturnsNull_When_NextOccurrenceIsBeyondMaxValue(string cronExpression, string fromString)
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);

            var fromWithOffset = GetInstantFromLocalTime(fromString, TimeZoneInfo.Utc);
            var fromUtc = fromWithOffset.UtcDateTime;

            var occurrenceDateTime = expression.GetNextOccurrence(fromUtc, TimeZoneInfo.Utc, inclusive: true);
            Assert.IsNull(occurrenceDateTime);

            var occurrenceWithOffset = expression.GetNextOccurrence(fromWithOffset, TimeZoneInfo.Utc);
            Assert.IsNull(occurrenceWithOffset);
        }

        [TestMethod]
        [DataRow("30 0 L  * *", "2017-03-30 23:59 +02:00", "2017-03-31 01:00 +03:00")]
        [DataRow("30 0 L  * *", "2017-03-31 01:00 +03:00", "2017-04-30 00:30 +03:00")]
        [DataRow("30 0 LW * *", "2018-03-29 23:59 +02:00", "2018-03-30 01:00 +03:00")]
        [DataRow("30 0 LW * *", "2018-03-30 01:00 +03:00", "2018-04-30 00:30 +03:00")]
        public void GetNextOccurrence_HandleDifficultDSTCases_WhenTheClockJumpsForwardOnFriday(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var occurrence = expression.GetNextOccurrence(fromInstant, JordanTimeZone, inclusive: true);

            // TODO: Rounding error.
            if (occurrence?.Millisecond == 999)
            {
                occurrence = occurrence.Value.AddMilliseconds(1);
            }

            Assert.AreEqual(expectedInstant, occurrence);
            Assert.AreEqual(expectedInstant.Offset, occurrence?.Offset);
        }

        [TestMethod]

        [DataRow("30 0 L  * *", "2014-10-31 00:30 +02:00", "2014-10-31 00:30 +02:00")]
        [DataRow("30 0 L  * *", "2014-10-31 00:30 +03:00", "2014-11-30 00:30 +02:00")]
        [DataRow("30 0 LW * *", "2015-10-30 00:30 +02:00", "2015-10-30 00:30 +02:00")]
        [DataRow("30 0 LW * *", "2015-10-30 00:30 +03:00", "2015-11-30 00:30 +02:00")]

        [DataRow("30 0 29 * *", "2019-03-28 23:59 +02:00", "2019-03-29 01:00 +03:00")]
        public void GetNextOccurrence_HandleDifficultDSTCases_WhenTheClockJumpsBackwardOnFriday(
            string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstant(fromString);
            var expectedInstant = GetInstant(expectedString);

            var occurrence = expression.GetNextOccurrence(fromInstant, JordanTimeZone, inclusive: true);

            // TODO: Rounding error.
            if (occurrence?.Millisecond == 999)
            {
                occurrence = occurrence.Value.AddMilliseconds(1);
            }

            Assert.AreEqual(expectedInstant, occurrence);
            Assert.AreEqual(expectedInstant.Offset, occurrence?.Offset);
        }

        //[TestMethod]
        ////[MemberData(nameof(GetTimeZones))]
        //public void GetNextOccurrence_ReturnsTheSameDateTimeWithGivenTimeZoneOffset(TimeZoneInfo zone)
        //{
        //    var fromInstant = new DateTimeOffset(2017, 03, 04, 00, 00, 00, new TimeSpan(12, 30, 00));
        //    var expectedInstant = fromInstant;

        //    var expectedOffset = zone.GetUtcOffset(expectedInstant);

        //    var occurrence = MinutelyExpression.GetNextOccurrence(fromInstant, zone, inclusive: true);

        //    Assert.AreEqual(expectedInstant, occurrence);
        //    Assert.AreEqual(expectedOffset, occurrence?.Offset);
        //}

        //[TestMethod]
        ////[MemberData(nameof(GetTimeZones))]
        //public void GetNextOccurrence_ReturnsUtcDateTime(TimeZoneInfo zone)
        //{
        //    var from = new DateTime(2017, 03, 06, 00, 00, 00, DateTimeKind.Utc);

        //    var occurrence = MinutelyExpression.GetNextOccurrence(from, zone, inclusive: true);

        //    Assert.AreEqual(from, occurrence);
        //    Assert.AreEqual(DateTimeKind.Utc, occurrence?.Kind);
        //}

        [TestMethod]

        [DataRow("* * 30    2    *    ", "1970-01-01")]
        [DataRow("* * 30-31 2    *    ", "1970-01-01")]
        [DataRow("* * 31    2    *    ", "1970-01-01")]
        [DataRow("* * 31    4    *    ", "1970-01-01")]
        [DataRow("* * 31    6    *    ", "1970-01-01")]
        [DataRow("* * 31    9    *    ", "1970-01-01")]
        [DataRow("* * 31    11   *    ", "1970-01-01")]
        [DataRow("* * L-30  11   *    ", "1970-01-01")]
        [DataRow("* * L-29  2    *    ", "1970-01-01")]
        [DataRow("* * L-30  2    *    ", "1970-01-01")]

        [DataRow("* * 1     *    SUN#2", "1970-01-01")]
        [DataRow("* * 7     *    SUN#2", "1970-01-01")]
        [DataRow("* * 1     *    SUN#3", "1970-01-01")]
        [DataRow("* * 14    *    SUN#3", "1970-01-01")]
        [DataRow("* * 1     *    SUN#4", "1970-01-01")]
        [DataRow("* * 21    *    SUN#4", "1970-01-01")]
        [DataRow("* * 1     *    SUN#5", "1970-01-01")]
        [DataRow("* * 28    *    SUN#5", "1970-01-01")]
        [DataRow("* * 1-28  *    SUN#5", "1970-01-01")]

        [DataRow("* * 8     *    MON#1", "1970-01-01")]
        [DataRow("* * 31    *    MON#1", "1970-01-01")]
        [DataRow("* * 15    *    TUE#2", "1970-01-01")]
        [DataRow("* * 31    *    TUE#2", "1970-01-01")]
        [DataRow("* * 22    *    WED#3", "1970-01-01")]
        [DataRow("* * 31    *    WED#3", "1970-01-01")]
        [DataRow("* * 29    *    THU#4", "1970-01-01")]
        [DataRow("* * 31    *    THU#4", "1970-01-01")]

        [DataRow("* * 21    *    7L   ", "1970-01-01")]
        [DataRow("* * 21    *    0L   ", "1970-01-01")]
        [DataRow("* * 11    *    0L   ", "1970-01-01")]
        [DataRow("* * 1     *    0L   ", "1970-01-01")]

        [DataRow("* * L     *    SUN#1", "1970-01-01")]
        [DataRow("* * L     *    SUN#2", "1970-01-01")]
        [DataRow("* * L     *    SUN#3", "1970-01-01")]
        [DataRow("* * L     1    SUN#4", "1970-01-01")]
        [DataRow("* * L     3-12 SUN#4", "1970-01-01")]

        [DataRow("* * L-1   2    SUN#5", "1970-01-01")]
        [DataRow("* * L-2   4    SUN#5", "1970-01-01")]
        [DataRow("* * L-3   *    SUN#5", "1970-01-01")]
        [DataRow("* * L-10  *    SUN#4", "1970-01-01")]

        [DataRow("* * 1W    *    SUN  ", "1970-01-01")]
        [DataRow("* * 4W    *    0    ", "1970-01-01")]
        [DataRow("* * 7W    *    7    ", "1970-01-01")]
        [DataRow("* * 5W    *    SAT  ", "1970-01-01")]

        [DataRow("* * 14W   *    6#2  ", "1970-01-01")]

        [DataRow("* * 7W    *    FRI#2", "1970-01-01")]
        [DataRow("* * 14W   *    TUE#3", "1970-01-01")]
        [DataRow("* * 11W   *    MON#3", "1970-01-01")]
        [DataRow("* * 21W   *    TUE#4", "1970-01-01")]
        [DataRow("* * 28W   *    SAT#5", "1970-01-01")]

        [DataRow("* * 21W   *    0L   ", "1970-01-01")]
        [DataRow("* * 19W   *    1L   ", "1970-01-01")]
        [DataRow("* * 1W    *    1L   ", "1970-01-01")]
        [DataRow("* * 21W   *    2L   ", "1970-01-01")]
        [DataRow("* * 2W    *    2L   ", "1970-01-01")]
        [DataRow("* * 21W   *    3L   ", "1970-01-01")]
        [DataRow("* * 3W    *    3L   ", "1970-01-01")]
        [DataRow("* * 21W   *    4L   ", "1970-01-01")]
        [DataRow("* * 4W    *    4L   ", "1970-01-01")]
        [DataRow("* * 21W   *    5L   ", "1970-01-01")]
        [DataRow("* * 5W    *    5L   ", "1970-01-01")]
        [DataRow("* * 21W   *    6L   ", "1970-01-01")]
        [DataRow("* * 21W   *    7L   ", "1970-01-01")]

        [DataRow("* * LW    *    SUN  ", "1970-01-01")]
        [DataRow("* * LW    *    0    ", "1970-01-01")]
        [DataRow("* * LW    *    0L   ", "1970-01-01")]
        [DataRow("* * LW    *    SAT  ", "1970-01-01")]
        [DataRow("* * LW    *    6    ", "1970-01-01")]
        [DataRow("* * LW    *    6L   ", "1970-01-01")]

        [DataRow("* * LW    *    1#1  ", "1970-01-01")]
        [DataRow("* * LW    *    2#2  ", "1970-01-01")]
        [DataRow("* * LW    *    3#3  ", "1970-01-01")]
        [DataRow("* * LW    1    4#4  ", "1970-01-01")]
        [DataRow("* * LW    3-12 4#4  ", "1970-01-01")]
        public void GetNextOccurrence_ReturnsNull_WhenCronExpressionIsUnreachable(string cronExpression, string fromString)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstantFromLocalTime(fromString, EasternTimeZone);

            var occurrence = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive: true);

            Assert.IsNull(occurrence);
        }

        [TestMethod]
        [DataRow("* * 30   2  *", "2080-01-01")]
        [DataRow("* * L-30 11 *", "2080-01-01")]
        public void GetNextOccurrence_ReturnsNull_WhenCronExpressionIsUnreachableAndFromIsDateTime(string cronExpression, string fromString)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstantFromLocalTime(fromString, TimeZoneInfo.Utc);

            var occurrence = expression.GetNextOccurrence(fromInstant.UtcDateTime);

            Assert.IsNull(occurrence);
        }

        #region 基础测试
        [TestMethod]

        // Basic facts.

        [DataRow("* * * * *", "17:35", "17:35")]

        [DataRow("* * * * *", "17:35:01", "17:36:00")]
        [DataRow("* * * * *", "17:35:59", "17:36:00")]
        [DataRow("* * * * *", "17:36:00", "17:36:00")]

        // Minute specified.

        [DataRow("12    * * * *", "15:05", "15:12")]
        [DataRow("12    * * * *", "15:12", "15:12")]
        [DataRow("12    * * * *", "15:59", "16:12")]
        [DataRow("31-39 * * * *", "15:00", "15:31")]
        [DataRow("31-39 * * * *", "15:30", "15:31")]
        [DataRow("31-39 * * * *", "15:31", "15:31")]
        [DataRow("31-39 * * * *", "15:39", "15:39")]
        [DataRow("31-39 * * * *", "15:59", "16:31")]
        [DataRow("*/20  * * * *", "15:00", "15:00")]
        [DataRow("*/20  * * * *", "15:10", "15:20")]
        [DataRow("*/20  * * * *", "15:59", "16:00")]
        [DataRow("10/5  * * * *", "15:00", "15:10")]
        [DataRow("10/5  * * * *", "15:14", "15:15")]
        [DataRow("10/5  * * * *", "15:59", "16:10")]
        [DataRow("0     * * * *", "15:59", "16:00")]

        [DataRow("44 * * * *", "19:44:01", "20:44:00")]
        [DataRow("44 * * * *", "19:44:30", "20:44:00")]
        [DataRow("44 * * * *", "19:44:59", "20:44:00")]
        [DataRow("44 * * * *", "19:45:00", "20:44:00")]

        [DataRow("5-8,19,20,35-41 * * * *", "15:01", "15:05")]
        [DataRow("5-8,19,20,35-41 * * * *", "15:06", "15:06")]
        [DataRow("5-8,19,20,35-41 * * * *", "15:18", "15:19")]
        [DataRow("5-8,19,20,35-41 * * * *", "15:19", "15:19")]
        [DataRow("5-8,19,20,35-41 * * * *", "15:20", "15:20")]
        [DataRow("5-8,19,20,35-41 * * * *", "15:21", "15:35")]
        [DataRow("5-8,19,20,35-41 * * * *", "15:36", "15:36")]
        [DataRow("5-8,19,20,35-41 * * * *", "15:42", "16:05")]

        [DataRow("51-4 * * * *", "17:35", "17:51")]
        [DataRow("51-4 * * * *", "17:51", "17:51")]
        [DataRow("51-4 * * * *", "17:55", "17:55")]
        [DataRow("51-4 * * * *", "17:59", "17:59")]
        [DataRow("51-4 * * * *", "18:00", "18:00")]
        [DataRow("51-4 * * * *", "18:04", "18:04")]
        [DataRow("51-4 * * * *", "18:05", "18:51")]

        [DataRow("56-4/4 * * * *", "17:55", "17:56")]
        [DataRow("56-4/4 * * * *", "17:57", "18:00")]
        [DataRow("56-4/4 * * * *", "18:01", "18:04")]
        [DataRow("56-4/4 * * * *", "18:05", "18:56")]

        [DataRow("45-44 * * * *", "18:45", "18:45")]
        [DataRow("45-44 * * * *", "18:55", "18:55")]
        [DataRow("45-44 * * * *", "18:59", "18:59")]
        [DataRow("45-44 * * * *", "19:00", "19:00")]
        [DataRow("45-44 * * * *", "19:44", "19:44")]

        // Hour specified.

        [DataRow("* 11   * * *", "10:59", "11:00")]
        [DataRow("* 11   * * *", "11:30", "11:30")]
        [DataRow("* 3-22 * * *", "01:40", "03:00")]
        [DataRow("* 3-22 * * *", "11:40", "11:40")]
        [DataRow("* */2  * * *", "00:00", "00:00")]
        [DataRow("* */2  * * *", "01:00", "02:00")]
        [DataRow("* 4/5  * * *", "00:45", "04:00")]
        [DataRow("* 4/5  * * *", "04:14", "04:14")]
        [DataRow("* 4/5  * * *", "05:00", "09:00")]

        [DataRow("* 3-5,10,11,13-17 * * *", "01:55", "03:00")]
        [DataRow("* 3-5,10,11,13-17 * * *", "04:55", "04:55")]
        [DataRow("* 3-5,10,11,13-17 * * *", "06:10", "10:00")]
        [DataRow("* 3-5,10,11,13-17 * * *", "10:55", "10:55")]
        [DataRow("* 3-5,10,11,13-17 * * *", "11:25", "11:25")]
        [DataRow("* 3-5,10,11,13-17 * * *", "12:30", "13:00")]
        [DataRow("* 3-5,10,11,13-17 * * *", "17:30", "17:30")]

        [DataRow("* 23-3/2 * * *", "17:30", "23:00")]
        [DataRow("* 23-3/2 * * *", "00:30", "01:00")]
        [DataRow("* 23-3/2 * * *", "02:00", "03:00")]
        [DataRow("* 23-3/2 * * *", "04:00", "23:00")]

        [DataRow("* 23-22 * * *", "22:10", "22:10")]
        [DataRow("* 23-22 * * *", "23:10", "23:10")]
        [DataRow("* 23-22 * * *", "00:10", "00:10")]
        [DataRow("* 23-22 * * *", "07:10", "07:10")]

        // Day of month specified.

        [DataRow("* * 9     * *", "2016-11-01", "2016-11-09")]
        [DataRow("* * 9     * *", "2016-11-09", "2016-11-09")]
        [DataRow("* * 09    * *", "2016-11-10", "2016-12-09")]
        [DataRow("* * */4   * *", "2016-12-01", "2016-12-01")]
        [DataRow("* * */4   * *", "2016-12-02", "2016-12-05")]
        [DataRow("* * */4   * *", "2016-12-06", "2016-12-09")]
        [DataRow("* * */3   * *", "2016-12-02", "2016-12-04")]
        [DataRow("* * 10,20 * *", "2016-12-09", "2016-12-10")]
        [DataRow("* * 10,20 * *", "2016-12-12", "2016-12-20")]
        [DataRow("* * 16-23 * *", "2016-12-01", "2016-12-16")]
        [DataRow("* * 16-23 * *", "2016-12-16", "2016-12-16")]
        [DataRow("* * 16-23 * *", "2016-12-18", "2016-12-18")]
        [DataRow("* * 16-23 * *", "2016-12-23", "2016-12-23")]
        [DataRow("* * 16-23 * *", "2016-12-24", "2017-01-16")]

        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-01", "2016-12-05")]
        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-05", "2016-12-05")]
        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-06", "2016-12-06")]
        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-08", "2016-12-08")]
        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-09", "2016-12-19")]
        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-20", "2016-12-20")]
        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-21", "2016-12-28")]
        [DataRow("* * 5-8,19,20,28-29 * *", "2016-12-30", "2017-01-05")]
        [DataRow("* * 5-8,19,20,29-30 * *", "2017-02-27", "2017-03-05")]

        [DataRow("* * 30-31 * *", "2016-02-27", "2016-03-30")]
        [DataRow("* * 30-31 * *", "2017-02-27", "2017-03-30")]
        [DataRow("* * 31    * *", "2017-04-27", "2017-05-31")]

        [DataRow("* * 20-5/5 * *", "2017-05-19", "2017-05-20")]
        [DataRow("* * 20-5/5 * *", "2017-05-21", "2017-05-25")]
        [DataRow("* * 20-5/5 * *", "2017-05-26", "2017-05-30")]
        [DataRow("* * 20-5/5 * *", "2017-06-01", "2017-06-04")]
        [DataRow("* * 20-5/5 * *", "2017-06-05", "2017-06-20")]

        [DataRow("* * 20-5/5 * *", "2017-07-01", "2017-07-04")]

        [DataRow("* * 20-5/5 * *", "2018-02-26", "2018-03-04")]

        // Month specified.

        [DataRow("* * * 11      *", "2016-10-09", "2016-11-01")]
        [DataRow("* * * 11      *", "2016-11-02", "2016-11-02")]
        [DataRow("* * * 11      *", "2016-12-02", "2017-11-01")]
        [DataRow("* * * 3,9     *", "2016-01-09", "2016-03-01")]
        [DataRow("* * * 3,9     *", "2016-06-09", "2016-09-01")]
        [DataRow("* * * 3,9     *", "2016-10-09", "2017-03-01")]
        [DataRow("* * * 5-11    *", "2016-01-01", "2016-05-01")]
        [DataRow("* * * 5-11    *", "2016-05-07", "2016-05-07")]
        [DataRow("* * * 5-11    *", "2016-07-12", "2016-07-12")]
        [DataRow("* * * 05-11   *", "2016-12-13", "2017-05-01")]
        [DataRow("* * * DEC     *", "2016-08-09", "2016-12-01")]
        [DataRow("* * * mar-dec *", "2016-02-09", "2016-03-01")]
        [DataRow("* * * mar-dec *", "2016-04-09", "2016-04-09")]
        [DataRow("* * * mar-dec *", "2016-12-09", "2016-12-09")]
        [DataRow("* * * */4     *", "2016-01-09", "2016-01-09")]
        [DataRow("* * * */4     *", "2016-02-09", "2016-05-01")]
        [DataRow("* * * */3     *", "2016-12-09", "2017-01-01")]
        [DataRow("* * * */5     *", "2016-12-09", "2017-01-01")]
        [DataRow("* * * APR-NOV *", "2016-12-09", "2017-04-01")]

        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-01-01", "2016-02-01")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-02-10", "2016-02-10")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-03-01", "2016-03-01")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-05-20", "2016-06-01")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-06-10", "2016-06-10")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-07-05", "2016-07-05")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-08-15", "2016-09-01")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-11-25", "2016-11-25")]
        [DataRow("* * * 2-4,JUN,7,SEP-nov *", "2016-12-01", "2017-02-01")]

        [DataRow("* * * 12-2 *", "2016-05-19", "2016-12-01")]
        [DataRow("* * * 12-2 *", "2017-01-19", "2017-01-19")]
        [DataRow("* * * 12-2 *", "2017-02-19", "2017-02-19")]
        [DataRow("* * * 12-2 *", "2017-03-19", "2017-12-01")]

        [DataRow("* * * 9-8/3 *", "2016-07-19", "2016-09-01")]
        [DataRow("* * * 9-8/3 *", "2016-10-19", "2016-12-01")]
        [DataRow("* * * 9-8/3 *", "2017-01-19", "2017-03-01")]
        [DataRow("* * * 9-8/3 *", "2017-04-19", "2017-06-01")]

        // Day of week specified.

        // Monday        Tuesday       Wednesday     Thursday      Friday        Saturday      Sunday
        //                                           2016-12-01    2016-12-02    2016-12-03    2016-12-04
        // 2016-12-05    2016-12-06    2016-12-07    2016-12-08    2016-12-09    2016-12-10    2016-12-11
        // 2016-12-12    2016-12-13    2016-12-14    2016-12-15    2016-12-16    2016-12-17    2016-12-18

        [DataRow("* * * * 5      ", "2016-12-07", "2016-12-09")]
        [DataRow("* * * * 5      ", "2016-12-09", "2016-12-09")]
        [DataRow("* * * * 05     ", "2016-12-10", "2016-12-16")]
        [DataRow("* * * * 3,5,7  ", "2016-12-09", "2016-12-09")]
        [DataRow("* * * * 3,5,7  ", "2016-12-10", "2016-12-11")]
        [DataRow("* * * * 3,5,7  ", "2016-12-12", "2016-12-14")]
        [DataRow("* * * * 4-7    ", "2016-12-08", "2016-12-08")]
        [DataRow("* * * * 4-7    ", "2016-12-10", "2016-12-10")]
        [DataRow("* * * * 4-7    ", "2016-12-11", "2016-12-11")]
        [DataRow("* * * * 4-07   ", "2016-12-12", "2016-12-15")]
        [DataRow("* * * * FRI    ", "2016-12-08", "2016-12-09")]
        [DataRow("* * * * tue/2  ", "2016-12-09", "2016-12-10")]
        [DataRow("* * * * tue/2  ", "2016-12-11", "2016-12-13")]
        [DataRow("* * * * FRI/3  ", "2016-12-03", "2016-12-09")]
        [DataRow("* * * * thu-sat", "2016-12-04", "2016-12-08")]
        [DataRow("* * * * thu-sat", "2016-12-09", "2016-12-09")]
        [DataRow("* * * * thu-sat", "2016-12-10", "2016-12-10")]
        [DataRow("* * * * thu-sat", "2016-12-12", "2016-12-15")]
        [DataRow("* * * * */5    ", "2016-12-08", "2016-12-09")]
        [DataRow("* * * * */5    ", "2016-12-10", "2016-12-11")]
        [DataRow("* * * * */5    ", "2016-12-12", "2016-12-16")]
        [DataRow("* * ? * thu-sun", "2016-12-09", "2016-12-09")]

        [DataRow("* * ? * sat-tue", "2016-12-10", "2016-12-10")]
        [DataRow("* * ? * sat-tue", "2016-12-11", "2016-12-11")]
        [DataRow("* * ? * sat-tue", "2016-12-12", "2016-12-12")]
        [DataRow("* * ? * sat-tue", "2016-12-13", "2016-12-13")]
        [DataRow("* * ? * sat-tue", "2016-12-14", "2016-12-17")]

        [DataRow("* * ? * sat-tue/2", "2016-12-10", "2016-12-10")]
        [DataRow("* * ? * sat-tue/2", "2016-12-11", "2016-12-12")]
        [DataRow("* * ? * sat-tue/2", "2016-12-12", "2016-12-12")]
        [DataRow("* * ? * sat-tue/2", "2016-12-13", "2016-12-17")]

        [DataRow("00 00 11 12 0  ", "2016-12-07", "2016-12-11")]
        [DataRow("00 00 11 12 7  ", "2016-12-09", "2016-12-11")]
        [DataRow("00 00 11 12 SUN", "2016-12-10", "2016-12-11")]
        [DataRow("00 00 11 12 sun", "2016-12-09", "2016-12-11")]

        // All fields are specified.

        [DataRow("47    17    09   12    5    ", "2016-10-01 00:00", "2016-12-09 17:47")]
        [DataRow("47    17    09   DEC   FRI  ", "2016-07-05 00:00", "2016-12-09 17:47")]
        [DataRow("40-50 15-20 5-10 11,12 5,6,7", "2016-12-01 00:00", "2016-12-09 15:40")]
        [DataRow("40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 15:40", "2016-12-09 15:40")]
        [DataRow("40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 15:45", "2016-12-09 15:45")]
        [DataRow("40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 15:51", "2016-12-09 16:40")]
        [DataRow("40-50 15-20 5-10 11,12 5,6,7", "2016-12-09 21:50", "2016-12-10 15:40")]
        [DataRow("40-50 15-20 5-10 11,12 5,6,7", "2016-12-11 21:50", "2017-11-05 15:40")]

        // Friday the thirteenth.

        [DataRow("05    18    13   01    05   ", "2016-01-01 00:00", "2017-01-13 18:05")]
        [DataRow("05    18    13   *     05   ", "2016-01-01 00:00", "2016-05-13 18:05")]
        [DataRow("05    18    13   *     05   ", "2016-09-01 00:00", "2017-01-13 18:05")]
        [DataRow("05    18    13   *     05   ", "2017-02-01 00:00", "2017-10-13 18:05")]

        // Handle moving to next second, minute, hour, month, year.

        [DataRow("0 * * * *", "2017-01-14 12:59", "2017-01-14 13:00")]
        [DataRow("0 0 * * *", "2017-01-14 23:00", "2017-01-15 00:00")]

        [DataRow("0 0 1 * *", "2016-02-10 00:00", "2016-03-01 00:00")]
        [DataRow("0 0 1 * *", "2017-02-10 00:00", "2017-03-01 00:00")]
        [DataRow("0 0 1 * *", "2017-04-10 00:00", "2017-05-01 00:00")]
        [DataRow("0 0 1 * *", "2017-01-30 00:00", "2017-02-01 00:00")]
        [DataRow("0 0 * * *", "2017-12-31 23:59", "2018-01-01 00:00")]

        // Skip month if day of month is specified and month has less days.

        [DataRow("0 0 30 * *", "2017-02-25 00:00", "2017-03-30 00:00")]
        [DataRow("0 0 31 * *", "2017-02-25 00:00", "2017-03-31 00:00")]
        [DataRow("0 0 31 * *", "2017-04-01 00:00", "2017-05-31 00:00")]

        // Leap year.

        [DataRow("0 0 29 2 *", "2016-03-10 00:00", "2020-02-29 00:00")]

        // Support 'L' character in day of month field.

        [DataRow("* * L * *", "2016-01-05", "2016-01-31")]
        [DataRow("* * L * *", "2016-01-31", "2016-01-31")]
        [DataRow("* * L * *", "2016-02-05", "2016-02-29")]
        [DataRow("* * L * *", "2016-02-29", "2016-02-29")]
        [DataRow("* * L 2 *", "2016-02-29", "2016-02-29")]
        [DataRow("* * L * *", "2017-02-28", "2017-02-28")]
        [DataRow("* * L * *", "2016-03-05", "2016-03-31")]
        [DataRow("* * L * *", "2016-03-31", "2016-03-31")]
        [DataRow("* * L * *", "2016-04-05", "2016-04-30")]
        [DataRow("* * L * *", "2016-04-30", "2016-04-30")]
        [DataRow("* * L * *", "2016-05-05", "2016-05-31")]
        [DataRow("* * L * *", "2016-05-31", "2016-05-31")]
        [DataRow("* * L * *", "2016-06-05", "2016-06-30")]
        [DataRow("* * L * *", "2016-06-30", "2016-06-30")]
        [DataRow("* * L * *", "2016-07-05", "2016-07-31")]
        [DataRow("* * L * *", "2016-07-31", "2016-07-31")]
        [DataRow("* * L * *", "2016-08-05", "2016-08-31")]
        [DataRow("* * L * *", "2016-08-31", "2016-08-31")]
        [DataRow("* * L * *", "2016-09-05", "2016-09-30")]
        [DataRow("* * L * *", "2016-09-30", "2016-09-30")]
        [DataRow("* * L * *", "2016-10-05", "2016-10-31")]
        [DataRow("* * L * *", "2016-10-31", "2016-10-31")]
        [DataRow("* * L * *", "2016-11-05", "2016-11-30")]
        [DataRow("* * L * *", "2016-12-05", "2016-12-31")]
        [DataRow("* * L * *", "2016-12-31", "2016-12-31")]
        [DataRow("* * L * *", "2099-12-05", "2099-12-31")]
        [DataRow("* * L * *", "2099-12-31", "2099-12-31")]

        [DataRow("* * L-1 * *", "2016-01-01", "2016-01-30")]
        [DataRow("* * L-1 * *", "2016-01-29", "2016-01-30")]
        [DataRow("* * L-1 * *", "2016-01-30", "2016-01-30")]
        [DataRow("* * L-1 * *", "2016-01-31", "2016-02-28")]
        [DataRow("* * L-1 * *", "2016-02-01", "2016-02-28")]
        [DataRow("* * L-1 * *", "2016-02-28", "2016-02-28")]
        [DataRow("* * L-1 * *", "2017-02-01", "2017-02-27")]
        [DataRow("* * L-1 * *", "2017-02-27", "2017-02-27")]
        [DataRow("* * L-1 * *", "2016-04-01", "2016-04-29")]
        [DataRow("* * L-1 * *", "2016-04-29", "2016-04-29")]
        [DataRow("* * L-1 * *", "2016-12-01", "2016-12-30")]

        [DataRow("* * L-2 * *", "2016-01-05", "2016-01-29")]
        [DataRow("* * L-2 * *", "2016-01-30", "2016-02-27")]
        [DataRow("* * L-2 * *", "2016-02-01", "2016-02-27")]
        [DataRow("* * L-2 * *", "2017-02-01", "2017-02-26")]
        [DataRow("* * L-2 * *", "2016-04-01", "2016-04-28")]
        [DataRow("* * L-2 * *", "2016-12-01", "2016-12-29")]
        [DataRow("* * L-2 * *", "2016-12-29", "2016-12-29")]
        [DataRow("* * L-2 * *", "2016-12-30", "2017-01-29")]

        [DataRow("* * L-28 * *", "2016-01-01", "2016-01-03")]
        [DataRow("* * L-28 * *", "2016-04-01", "2016-04-02")]
        [DataRow("* * L-28 * *", "2016-02-01", "2016-02-01")]
        [DataRow("* * L-28 * *", "2017-02-01", "2017-03-03")]

        [DataRow("* * L-29 * *", "2016-01-01", "2016-01-02")]
        [DataRow("* * L-29 * *", "2016-04-01", "2016-04-01")]
        [DataRow("* * L-29 * *", "2016-02-01", "2016-03-02")]
        [DataRow("* * L-29 * *", "2017-02-01", "2017-03-02")]

        [DataRow("* * L-30 * *", "2016-01-01", "2016-01-01")]
        [DataRow("* * L-30 * *", "2016-04-01", "2016-05-01")]
        [DataRow("* * L-30 * *", "2016-02-01", "2016-03-01")]
        [DataRow("* * L-30 * *", "2017-02-01", "2017-03-01")]

        // Support 'L' character in day of week field.

        // Monday        Tuesday       Wednesday     Thursday      Friday        Saturday      Sunday
        // 2016-01-23    2016-01-24    2016-01-25    2016-01-26    2016-01-27    2016-01-28    2016-01-29
        // 2016-01-30    2016-01-31

        [DataRow("* * * * 0L", "2017-01-29", "2017-01-29")]
        [DataRow("* * * * 0L", "2017-01-01", "2017-01-29")]
        [DataRow("* * * * 1L", "2017-01-30", "2017-01-30")]
        [DataRow("* * * * 1L", "2017-01-01", "2017-01-30")]
        [DataRow("* * * * 2L", "2017-01-31", "2017-01-31")]
        [DataRow("* * * * 2L", "2017-01-01", "2017-01-31")]
        [DataRow("* * * * 3L", "2017-01-25", "2017-01-25")]
        [DataRow("* * * * 3L", "2017-01-01", "2017-01-25")]
        [DataRow("* * * * 4L", "2017-01-26", "2017-01-26")]
        [DataRow("* * * * 4L", "2017-01-01", "2017-01-26")]
        [DataRow("* * * * 5L", "2017-01-27", "2017-01-27")]
        [DataRow("* * * * 5L", "2017-01-01", "2017-01-27")]
        [DataRow("* * * * 6L", "2017-01-28", "2017-01-28")]
        [DataRow("* * * * 6L", "2017-01-01", "2017-01-28")]
        [DataRow("* * * * 7L", "2017-01-29", "2017-01-29")]
        [DataRow("* * * * 7L", "2016-12-31", "2017-01-29")]

        // Support '#' in day of week field.

        [DataRow("* * * * SUN#1", "2017-01-01", "2017-01-01")]
        [DataRow("* * * * 0#1  ", "2017-01-01", "2017-01-01")]
        [DataRow("* * * * 0#1  ", "2016-12-10", "2017-01-01")]
        [DataRow("* * * * 0#1  ", "2017-02-01", "2017-02-05")]
        [DataRow("* * * * 0#2  ", "2017-01-01", "2017-01-08")]
        [DataRow("* * * * 0#2  ", "2017-01-08", "2017-01-08")]
        [DataRow("* * * * 5#3  ", "2017-01-01", "2017-01-20")]
        [DataRow("* * * * 5#3  ", "2017-01-21", "2017-02-17")]
        [DataRow("* * * * 3#2  ", "2017-01-01", "2017-01-11")]
        [DataRow("* * * * 2#5  ", "2017-02-01", "2017-05-30")]

        // Support 'W' in day of month field.

        [DataRow("* * 1W * *", "2017-01-01", "2017-01-02")]
        [DataRow("* * 2W * *", "2017-01-02", "2017-01-02")]
        [DataRow("* * 6W * *", "2017-01-02", "2017-01-06")]
        [DataRow("* * 7W * *", "2017-01-02", "2017-01-06")]
        [DataRow("* * 7W * *", "2017-01-07", "2017-02-07")]
        [DataRow("* * 8W * *", "2017-01-02", "2017-01-09")]

        [DataRow("* * 30W * *", "2017-04-27", "2017-04-28")]
        [DataRow("* * 30W * *", "2017-04-28", "2017-04-28")]
        [DataRow("* * 30W * *", "2017-04-29", "2017-05-30")]

        [DataRow("* * 1W * *", "2017-04-01", "2017-04-03")]

        [DataRow("30    17 7W * *", "2017-01-06 17:45", "2017-02-07 17:30")]
        [DataRow("30,45 17 7W * *", "2017-01-06 17:45", "2017-01-06 17:45")]
        [DataRow("30,55 17 7W * *", "2017-01-06 17:45", "2017-01-06 17:55")]

        [DataRow("30    17 30W * *", "2017-04-28 17:45", "2017-05-30 17:30")]
        [DataRow("30,45 17 30W * *", "2017-04-28 17:45", "2017-04-28 17:45")]
        [DataRow("30,55 17 30W * *", "2017-04-28 17:45", "2017-04-28 17:55")]

        [DataRow("30    17 30W * *", "2017-02-06 00:00", "2017-03-30 17:30")]

        [DataRow("30    17 31W * *", "2018-03-30 17:45", "2018-05-31 17:30")]
        [DataRow("30    17 15W * *", "2016-12-30 17:45", "2017-01-16 17:30")]

        // Support 'LW' in day of month field.

        [DataRow("* * LW * *", "2017-01-01", "2017-01-31")]
        [DataRow("* * LW * *", "2017-09-01", "2017-09-29")]
        [DataRow("* * LW * *", "2017-09-29", "2017-09-29")]
        [DataRow("* * LW * *", "2017-09-30", "2017-10-31")]
        [DataRow("* * LW * *", "2017-04-01", "2017-04-28")]
        [DataRow("* * LW * *", "2017-04-28", "2017-04-28")]
        [DataRow("* * LW * *", "2017-04-29", "2017-05-31")]
        [DataRow("* * LW * *", "2017-05-30", "2017-05-31")]

        [DataRow("30 17 LW * *", "2017-09-29 17:45", "2017-10-31 17:30")]

        [DataRow("* * L-1W * *", "2017-01-01", "2017-01-30")]
        [DataRow("* * L-2W * *", "2017-01-01", "2017-01-30")]
        [DataRow("* * L-3W * *", "2017-01-01", "2017-01-27")]
        [DataRow("* * L-4W * *", "2017-01-01", "2017-01-27")]

        [DataRow("* * L-0W * *", "2016-02-01", "2016-02-29")]
        [DataRow("* * L-0W * *", "2017-02-01", "2017-02-28")]
        [DataRow("* * L-1W * *", "2016-02-01", "2016-02-29")]
        [DataRow("* * L-1W * *", "2017-02-01", "2017-02-27")]
        [DataRow("* * L-2W * *", "2016-02-01", "2016-02-26")]
        [DataRow("* * L-2W * *", "2017-02-01", "2017-02-27")]
        [DataRow("* * L-3W * *", "2016-02-01", "2016-02-26")]
        [DataRow("* * L-3W * *", "2017-02-01", "2017-02-24")]

        // Support '?'.

        [DataRow("* * ? 11 *", "2016-10-09", "2016-11-01")]

        [DataRow("? ? ? ? ?", "2016-12-09 16:46", "2016-12-09 16:46")]
        [DataRow("* * * * ?", "2016-12-09 16:46", "2016-12-09 16:46")]
        [DataRow("* * ? * *", "2016-03-09 16:46", "2016-03-09 16:46")]
        [DataRow("* * * * ?", "2016-12-30 16:46", "2016-12-30 16:46")]
        [DataRow("* * ? * *", "2016-12-09 02:46", "2016-12-09 02:46")]
        [DataRow("* * * * ?", "2016-12-09 16:09", "2016-12-09 16:09")]
        [DataRow("* * ? * *", "2099-12-09 16:46", "2099-12-09 16:46")]
        public void GetNextOccurrence_ReturnsCorrectDate_WhenExpressionContains5FieldsAndInclusiveIsTrue(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstantFromLocalTime(fromString, EasternTimeZone);

            var occurrence = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive: true);

            Assert.AreEqual(GetInstantFromLocalTime(expectedString, EasternTimeZone), occurrence);
        }
        #endregion

        [TestMethod]

        [DataRow("@every_second", "2017-03-23 16:46:05", "2017-03-23 16:46:05")]

        [DataRow("@every_minute", "2017-03-23 16:46", "2017-03-23 16:46")]
        [DataRow("@hourly      ", "2017-03-23 16:46", "2017-03-23 17:00")]
        [DataRow("@daily       ", "2017-03-23 16:46", "2017-03-24 00:00")]
        [DataRow("@midnight    ", "2017-03-23 16:46", "2017-03-24 00:00")]
        [DataRow("@monthly     ", "2017-03-23 16:46", "2017-04-01 00:00")]
        [DataRow("@yearly      ", "2017-03-23 16:46", "2018-01-01 00:00")]
        [DataRow("@annually    ", "2017-03-23 16:46", "2018-01-01 00:00")]

        // Case-insensitive.
        [DataRow("@EVERY_SECOND", "2017-03-23 16:46:05", "2017-03-23 16:46:05")]

        [DataRow("@EVERY_MINUTE", "2017-03-23 16:46", "2017-03-23 16:46")]
        [DataRow("@HOURLY      ", "2017-03-23 16:46", "2017-03-23 17:00")]
        [DataRow("@DAILY       ", "2017-03-23 16:46", "2017-03-24 00:00")]
        [DataRow("@MIDNIGHT    ", "2017-03-23 16:46", "2017-03-24 00:00")]
        [DataRow("@MONTHLY     ", "2017-03-23 16:46", "2017-04-01 00:00")]
        [DataRow("@YEARLY      ", "2017-03-23 16:46", "2018-01-01 00:00")]
        [DataRow("@ANNUALLY    ", "2017-03-23 16:46", "2018-01-01 00:00")]
        public void GetNextOccurrence_ReturnsCorrectDate_WhenExpressionIsMacros(string cronExpression, string fromString, string expectedString)
        {
            var expression = CronExpression.Parse(cronExpression);

            var fromInstant = GetInstantFromLocalTime(fromString, EasternTimeZone);

            var occurrence = expression.GetNextOccurrence(fromInstant, EasternTimeZone, inclusive: true);

            Assert.AreEqual(GetInstantFromLocalTime(expectedString, EasternTimeZone), occurrence);
        }

        [TestMethod]
        [DataRow("* * * * *", "2017-03-16 16:00", "2017-03-16 16:01")]
        [DataRow("5 * * * *", "2017-03-16 16:05", "2017-03-16 17:05")]
        [DataRow("* 5 * * *", "2017-03-16 05:00", "2017-03-16 05:01")]
        [DataRow("* * 5 * *", "2017-03-05 16:00", "2017-03-05 16:01")]
        [DataRow("* * * 5 *", "2017-05-16 16:00", "2017-05-16 16:01")]
        [DataRow("* * * * 5", "2017-03-17 16:00", "2017-03-17 16:01")]
        [DataRow("5 5 * * *", "2017-03-16 05:05", "2017-03-17 05:05")]
        [DataRow("5 5 5 * *", "2017-03-05 05:05", "2017-04-05 05:05")]
        [DataRow("5 5 5 5 *", "2017-05-05 05:05", "2018-05-05 05:05")]
        [DataRow("5 5 5 5 5", "2017-05-05 05:05", "2023-05-05 05:05")]
        public void GetNextOccurrence_ReturnsCorrectDate_WhenFromIsDateTimeOffsetAndInclusiveIsFalse(string expression, string from, string expectedString)
        {
            var cronExpression = CronExpression.Parse(expression);

            var fromInstant = GetInstantFromLocalTime(from, EasternTimeZone);

            var nextOccurrence = cronExpression.GetNextOccurrence(fromInstant, EasternTimeZone);

            Assert.AreEqual(GetInstantFromLocalTime(expectedString, EasternTimeZone), nextOccurrence);
        }

        [TestMethod]
        [DataRow("* * * * *", "2017-03-16 16:00", "2017-03-16 16:01")]
        [DataRow("5 * * * *", "2017-03-16 16:05", "2017-03-16 17:05")]
        [DataRow("* 5 * * *", "2017-03-16 05:00", "2017-03-16 05:01")]
        [DataRow("* * 5 * *", "2017-03-05 16:00", "2017-03-05 16:01")]
        [DataRow("* * * 5 *", "2017-05-16 16:00", "2017-05-16 16:01")]
        [DataRow("* * * * 5", "2017-03-17 16:00", "2017-03-17 16:01")]
        [DataRow("5 5 * * *", "2017-03-16 05:05", "2017-03-17 05:05")]
        [DataRow("5 5 5 * *", "2017-03-05 05:05", "2017-04-05 05:05")]
        [DataRow("5 5 5 5 *", "2017-05-05 05:05", "2018-05-05 05:05")]
        [DataRow("5 5 5 5 5", "2017-05-05 05:05", "2023-05-05 05:05")]
        public void GetNextOccurrence_ReturnsCorrectDate_WhenFromIsDateTimeAndZoneIsSpecifiedAndInclusiveIsFalse(string expression, string fromString, string expectedString)
        {
            var cronExpression = CronExpression.Parse(expression);

            var fromInstant = GetInstantFromLocalTime(fromString, EasternTimeZone);
            var expectedInstant = GetInstantFromLocalTime(expectedString, EasternTimeZone);

            var nextOccurrence = cronExpression.GetNextOccurrence(fromInstant.UtcDateTime, EasternTimeZone);

            Assert.AreEqual(expectedInstant.UtcDateTime, nextOccurrence);
        }

        [TestMethod]
        [DataRow("* * * * * *", "1991-01-01 00:00")]
        [DataRow("0 * * * * *", "1991-03-02 00:00")]
        [DataRow("* 0 * * * *", "1991-03-15 00:00")]
        [DataRow("* * 0 * * *", "1991-03-31 00:00")]
        [DataRow("* * * 1 * *", "1991-04-15 00:00")]
        [DataRow("* * * * 1 *", "1991-05-25 00:00")]
        [DataRow("* * * * * 0", "1991-06-27 00:00")]
        [DataRow("0 0 0 * * *", "1991-07-16 00:00")]
        [DataRow("0 0 0 1 * *", "1991-10-30 00:00")]
        [DataRow("0 0 0 1 1 *", "1991-12-31 00:00")]
        [DataRow("0 0 0 1 * 1", "1991-12-31 00:00")]
        public void GetNextOccurrence_MakesProgressInsideLoop(string expression, string fromString)
        {
            var cronExpression = CronExpression.Parse(expression, CronFormat.IncludeSeconds);

            var fromInstant = GetInstantFromLocalTime(fromString, EasternTimeZone);

            for (var i = 0; i < 100; i++)
            {
                var nextOccurrence = cronExpression.GetNextOccurrence(fromInstant.AddTicks(1), EasternTimeZone, inclusive: true);

                Assert.IsTrue(nextOccurrence > fromInstant);

                fromInstant = nextOccurrence.Value;
            }
        }

        [TestMethod]
        public void GetNextOccurrence_ReturnsAGreaterValue_EvenWhenMillisecondTruncationRuleIsAppliedDueToDST()
        {
            var expression = CronExpression.Parse("* * * * * *", CronFormat.IncludeSeconds);
            var fromInstant = DateTimeOffset.Parse("2021-03-25 23:59:59.9999999 +02:00");

            var nextInstant = expression.GetNextOccurrence(fromInstant, JordanTimeZone, inclusive: true);

            Assert.IsTrue(nextInstant > fromInstant);
        }

        [TestMethod]
        [DataRow("* * * * *", "2017-03-16 16:00", "2017-03-16 16:01")]
        [DataRow("5 * * * *", "2017-03-16 16:05", "2017-03-16 17:05")]
        [DataRow("* 5 * * *", "2017-03-16 05:00", "2017-03-16 05:01")]
        [DataRow("* * 5 * *", "2017-03-05 16:00", "2017-03-05 16:01")]
        [DataRow("* * * 5 *", "2017-05-16 16:00", "2017-05-16 16:01")]
        [DataRow("* * * * 5", "2017-03-17 16:00", "2017-03-17 16:01")]
        [DataRow("5 5 * * *", "2017-03-16 05:05", "2017-03-17 05:05")]
        [DataRow("5 5 5 * *", "2017-03-05 05:05", "2017-04-05 05:05")]
        [DataRow("5 5 5 5 *", "2017-05-05 05:05", "2018-05-05 05:05")]
        [DataRow("5 5 5 5 5", "2017-05-05 05:05", "2023-05-05 05:05")]
        public void GetNextOccurrence_ReturnsCorrectDate_WhenFromIsUtcDateTimeAndInclusiveIsFalse(string expression, string fromString, string expectedString)
        {
            var cronExpression = CronExpression.Parse(expression);

            var fromInstant = GetInstantFromLocalTime(fromString, TimeZoneInfo.Utc);
            var expectedInstant = GetInstantFromLocalTime(expectedString, TimeZoneInfo.Utc);

            var nextOccurrence = cronExpression.GetNextOccurrence(fromInstant.UtcDateTime);

            Assert.AreEqual(expectedInstant.UtcDateTime, nextOccurrence);
        }

        [TestMethod]
        [DataRow("* * * * * *", "2017-03-16 16:00:00", "2017-03-16 16:00:01")]
        [DataRow("5 * * * * *", "2017-03-16 16:00:05", "2017-03-16 16:01:05")]
        [DataRow("* 5 * * * *", "2017-03-16 16:05:00", "2017-03-16 16:05:01")]
        [DataRow("* * 5 * * *", "2017-03-16 05:00:00", "2017-03-16 05:00:01")]
        [DataRow("* * * 5 * *", "2017-03-05 16:00:00", "2017-03-05 16:00:01")]
        [DataRow("* * * * 5 *", "2017-05-16 16:00:00", "2017-05-16 16:00:01")]
        [DataRow("* * * * * 5", "2017-03-17 16:00:00", "2017-03-17 16:00:01")]
        [DataRow("5 5 * * * *", "2017-03-16 16:05:05", "2017-03-16 17:05:05")]
        [DataRow("5 5 5 * * *", "2017-03-16 05:05:05", "2017-03-17 05:05:05")]
        [DataRow("5 5 5 5 * *", "2017-03-05 05:05:05", "2017-04-05 05:05:05")]
        [DataRow("5 5 5 5 5 *", "2017-05-05 05:05:05", "2018-05-05 05:05:05")]
        [DataRow("5 5 5 5 5 5", "2017-05-05 05:05:05", "2023-05-05 05:05:05")]
        public void GetNextOccurrence_ReturnsCorrectDate_When6fieldsExpressionIsUsedAndInclusiveIsFalse(string expression, string fromString, string expectedString)
        {
            var cronExpression = CronExpression.Parse(expression, CronFormat.IncludeSeconds);

            var from = GetInstantFromLocalTime(fromString, EasternTimeZone);

            var nextOccurrence = cronExpression.GetNextOccurrence(from, EasternTimeZone);

            Assert.AreEqual(GetInstantFromLocalTime(expectedString, EasternTimeZone), nextOccurrence);
        }

        [TestMethod]
        public void GetOccurrences_DateTime_ThrowsAnException_WhenFromGreaterThanTo()
        {
            var expression = CronExpression.Parse("* * * * *");
            Assert.ThrowsException<ArgumentException>(() => expression.GetOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(-5)).ToArray());
        }

        [TestMethod]
        public void GetOccurrences_DateTime_ReturnsEmptyEnumerable_WhenNoOccurrencesFound()
        {
            var expression = CronExpression.Parse("* * 30 FEB *");

            var occurrences = expression.GetOccurrences(
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1));
            
            Assert.IsTrue(occurrences is null || !occurrences.Any());
        }

        [TestMethod]
        public void GetOccurrences_DateTime_ReturnsCollectionThatDoesNotIncludeToByDefault()
        {
            var expression = CronExpression.Parse("* 00 26 04 *");
            var from = new DateTime(2017, 04, 26, 00, 00, 00, 000, DateTimeKind.Utc);

            var occurrences = expression.GetOccurrences(from, from.AddMinutes(2)).ToArray();

            Assert.AreEqual(2, occurrences.Length);
            Assert.AreEqual(from, occurrences[0]);
            Assert.AreEqual(from.AddMinutes(1), occurrences[1]);
        }

        [TestMethod]
        public void GetOccurrences_DateTime_HandlesFromExclusiveArgument()
        {
            var expression = CronExpression.Parse("* 00 26 04 *");
            var from = new DateTime(2017, 04, 26, 00, 00, 00, 000, DateTimeKind.Utc);

            var occurrences = expression
                .GetOccurrences(from, from.AddMinutes(2), fromInclusive: false)
                .ToArray();

            Assert.IsTrue(occurrences.Length == 1);
            Assert.AreEqual(from.AddMinutes(1), occurrences[0]);
        }

        [TestMethod]
        public void GetOccurrences_DateTime_HandlesToInclusiveArgument()
        {
            var expression = CronExpression.Parse("* 00 26 04 *");
            var from = new DateTime(2017, 04, 26, 00, 00, 00, 000, DateTimeKind.Utc);

            var occurrences = expression
                .GetOccurrences(from, from.AddMinutes(2), toInclusive: true)
                .ToArray();

            Assert.AreEqual(3, occurrences.Length);
            Assert.AreEqual(from.AddMinutes(2), occurrences[2]);
        }

        [TestMethod]
        public void GetOccurrences_DateTimeTimeZone_ThrowsAnException_WhenFromGreaterThanTo()
        {
            var expression = CronExpression.Parse("* * * * *");
            Assert.ThrowsException<ArgumentException>(() => expression.GetOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(-5), EasternTimeZone).ToArray());
        }

        [TestMethod]
        public void GetOccurrences_DateTimeTimeZone_ReturnsEmptyEnumerable_WhenNoOccurrencesFound()
        {
            var expression = CronExpression.Parse("* * 30 FEB *");

            var occurrences = expression.GetOccurrences(
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1),
                EasternTimeZone);

            Assert.IsTrue(occurrences is null || !occurrences.Any());
        }

        [TestMethod]
        public void GetOccurrences_DateTimeTimeZone_ReturnsCollectionThatDoesNotIncludeToByDefault()
        {
            var expression = CronExpression.Parse("* 20 25 04 *");
            var from = new DateTime(2017, 04, 26, 00, 00, 00, 000, DateTimeKind.Utc);

            var occurrences = expression.GetOccurrences(from, from.AddMinutes(2), EasternTimeZone).ToArray();

            Assert.AreEqual(2, occurrences.Length);
            Assert.AreEqual(from, occurrences[0]);
            Assert.AreEqual(from.AddMinutes(1), occurrences[1]);
        }

        [TestMethod]
        public void GetOccurrences_DateTimeTimeZone_HandlesFromExclusiveArgument()
        {
            var expression = CronExpression.Parse("* 20 25 04 *");
            var from = new DateTime(2017, 04, 26, 00, 00, 00, 000, DateTimeKind.Utc);

            var occurrences = expression
                .GetOccurrences(from, from.AddMinutes(2), EasternTimeZone, fromInclusive: false)
                .ToArray();

            Assert.IsTrue(occurrences.Length == 1);
            Assert.AreEqual(from.AddMinutes(1), occurrences[0]);
        }

        [TestMethod]
        public void GetOccurrences_DateTimeTimeZone_HandlesToInclusiveArgument()
        {
            var expression = CronExpression.Parse("* 20 25 04 *");
            var from = new DateTime(2017, 04, 26, 00, 00, 00, 000, DateTimeKind.Utc);

            var occurrences = expression
                .GetOccurrences(from, from.AddMinutes(2), EasternTimeZone, toInclusive: true)
                .ToArray();

            Assert.AreEqual(3, occurrences.Length);
            Assert.AreEqual(from.AddMinutes(2), occurrences[2]);
        }

        [TestMethod]
        public void GetOccurrences_DateTimeOffset_ThrowsAnException_WhenFromGreaterThanTo()
        {
            var expression = CronExpression.Parse("* * * * *");
            Assert.ThrowsException<ArgumentException>(() => expression.GetOccurrences(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(-5), EasternTimeZone).ToArray());
        }

        [TestMethod]
        public void GetOccurrences_DateTimeOffset_ReturnsEmptyEnumerable_WhenNoOccurrencesFound()
        {
            var expression = CronExpression.Parse("* * 30 FEB *");

            var occurrences = expression.GetOccurrences(
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddYears(1),
                EasternTimeZone);

            Assert.IsTrue(occurrences is null || !occurrences.Any());
        }

        [TestMethod]
        public void GetOccurrences_DateTimeOffset_ReturnsCollectionThatDoesNotIncludeToByDefault()
        {
            var expression = CronExpression.Parse("* 20 25 04 *");
            var from = new DateTimeOffset(2017, 04, 26, 00, 00, 00, 000, TimeSpan.Zero);

            var occurrences = expression
                .GetOccurrences(from, from.AddMinutes(2), EasternTimeZone)
                .ToArray();

            Assert.AreEqual(2, occurrences.Length);
            Assert.AreEqual(from, occurrences[0]);
            Assert.AreEqual(from.AddMinutes(1), occurrences[1].UtcDateTime);
        }

        [TestMethod]
        public void GetOccurrences_DateTimeOffset_HandlesFromExclusiveArgument()
        {
            var expression = CronExpression.Parse("* 20 25 04 *");
            var from = new DateTimeOffset(2017, 04, 26, 00, 00, 00, 000, TimeSpan.Zero);

            var occurrences = expression
                .GetOccurrences(from, from.AddMinutes(2), EasternTimeZone, fromInclusive: false)
                .ToArray();

            Assert.IsTrue(occurrences.Length == 1);
            Assert.AreEqual(from.AddMinutes(1), occurrences[0].UtcDateTime);
        }

        [TestMethod]
        public void GetOccurrences_DateTimeOffset_HandlesToInclusiveArgument()
        {
            var expression = CronExpression.Parse("* 20 25 04 *");
            var from = new DateTimeOffset(2017, 04, 26, 00, 00, 00, 000, TimeSpan.Zero);

            var occurrences = expression
                .GetOccurrences(from, from.AddMinutes(2), EasternTimeZone, toInclusive: true)
                .ToArray();

            Assert.AreEqual(3, occurrences.Length);
            Assert.AreEqual(from.AddMinutes(2), occurrences[2].UtcDateTime);
        }

        [TestMethod]
        [DataRow("* * * * *", "* * * * *")]

        [DataRow("* * * * *", "0/2,1/2    * * * *")]
        [DataRow("* * * * *", "1/2,0-59/2 * * * *")]
        [DataRow("* * * * *", "0-59       * * * *")]
        [DataRow("* * * * *", "0,1,2-59   * * * *")]
        [DataRow("* * * * *", "0-59/1     * * * *")]
        [DataRow("* * * * *", "50-49      * * * *")]

        [DataRow("* * * * *", "* 0/3,2/3,1/3 * * *")]
        [DataRow("* * * * *", "* 0-23/2,1/2  * * *")]
        [DataRow("* * * * *", "* 0-23        * * *")]
        [DataRow("* * * * *", "* 0-23/1      * * *")]
        [DataRow("* * * * *", "* 12-11       * * *")]

        [DataRow("* * * * *", "* * 1/2,2/2     * *")]
        [DataRow("* * * * *", "* * 1-31/2,2/2  * *")]
        [DataRow("* * * * *", "* * 1-31        * *")]
        [DataRow("* * * * *", "* * 1-31/1      * *")]
        [DataRow("* * * * *", "* * 5-4         * *")]

        [DataRow("* * * * *", "* * * 1/2,2/2    *")]
        [DataRow("* * * * *", "* * * 1-12/2,2/2 *")]
        [DataRow("* * * * *", "* * * 1-12       *")]
        [DataRow("* * * * *", "* * * 1-12/1     *")]
        [DataRow("* * * * *", "* * * 12-11      *")]

        [DataRow("* * * * *", "* * * * 0/2,1/2    ")]
        [DataRow("* * * * *", "* * * * SUN/2,MON/2")]
        [DataRow("* * * * *", "* * * * 0-6/2,1/2  ")]
        [DataRow("* * * * *", "* * * * 0-7/2,1/2  ")]
        [DataRow("* * * * *", "* * * * 0-7/2,MON/2")]
        [DataRow("* * * * *", "* * * * 0-6        ")]
        [DataRow("* * * * *", "* * * * 0-7        ")]
        [DataRow("* * * * *", "* * * * SUN-SAT    ")]
        [DataRow("* * * * *", "* * * * 0-6/1      ")]
        [DataRow("* * * * *", "* * * * 0-7/1      ")]
        [DataRow("* * * * *", "* * * * SUN-SAT/1  ")]
        [DataRow("* * * * *", "* * * * MON-SUN    ")]

        [DataRow("* * *     * 0  ", "* * *     * 7  ")]
        [DataRow("* * *     * 0  ", "* * *     * SUN")]
        [DataRow("* * LW    * *  ", "* * LW    * *  ")]
        [DataRow("* * L-20W * 2  ", "* * L-20W * 2  ")]
        [DataRow("* * *     * 0#1", "* * *     * 0#1")]
        [DataRow("* * *     * 0L ", "* * *     * 7L ")]
        [DataRow("* * L-3W  * 0L ", "* * L-3W  * 0L ")]
        [DataRow("1 1 1     1 1  ", "1 1 1     1 1  ")]
        [DataRow("* * *     * *  ", "* * ?     * *  ")]
        [DataRow("* * *     * *  ", "* * *     * ?  ")]

        [DataRow("1-5 * * * *", "1-5 * * * *")]
        [DataRow("1-5 * * * *", "1-5/1 * * * *")]
        [DataRow("* * * * *", "0/1 * * * *")]
        [DataRow("1 * * * *", "1-1 * * * *")]
        [DataRow("*/4 * * * *", "0-59/4 * * * *")]

        [DataRow("1-5 1-5 1-5 1-5 1-5", "1-5 1-5 1-5 1-5 1-5")]

        [DataRow("50-15 * * * *", "50-15      * * * *")]
        [DataRow("50-15 * * * *", "0-15,50-59 * * * *")]

        [DataRow("* 20-15 * * *", "* 20-15      * * *")]
        [DataRow("* 20-15 * * *", "* 0-15,20-23 * * *")]

        [DataRow("* * 20-15 * *", "* * 20-15      * *")]
        [DataRow("* * 20-15 * *", "* * 1-15,20-31 * *")]

        [DataRow("* * * 10-3 *", "* * * 10-3      *")]
        [DataRow("* * * 10-3 *", "* * * 1-3,10-12 *")]

        [DataRow("* * * * 5-2", "* * * * 5-2    ")]
        [DataRow("* * * * 5-2", "* * * * 0-2,5-7")]
        [DataRow("* * * * 5-2", "* * * * 0-2,5-6")]
        [DataRow("* * * * 5-2", "* * * * 1-2,5-7")]

        [DataRow("* * * * FRI-TUE", "* * * * FRI-TUE        ")]
        [DataRow("* * * * FRI-TUE", "* * * * SUN-TUE,FRI-SUN")]
        [DataRow("* * * * FRI-TUE", "* * * * SUN-TUE,FRI-SAT")]
        [DataRow("* * * * FRI-TUE", "* * * * MON-TUE,FRI-SUN")]
        public void Equals_ReturnsTrue_WhenCronExpressionsAreEqual(string leftExpression, string rightExpression)
        {
            var leftCronExpression = CronExpression.Parse(leftExpression);
            var rightCronExpression = CronExpression.Parse(rightExpression);

            Assert.IsTrue(leftCronExpression.Equals(rightCronExpression));
            Assert.IsTrue(leftCronExpression == rightCronExpression);
            Assert.IsFalse(leftCronExpression != rightCronExpression);
            Assert.IsTrue(leftCronExpression.GetHashCode() == rightCronExpression.GetHashCode());
        }

        [TestMethod]
        public void Equals_ReturnsFalse_WhenOtherIsNull()
        {
            var cronExpression = CronExpression.Parse("* * * * *");

            Assert.IsFalse(cronExpression.Equals(null));
        }

        [TestMethod]
        [DataRow("1 1 1 1 1", "2 1 1 1 1")]
        [DataRow("1 1 1 1 1", "1 2 1 1 1")]
        [DataRow("1 1 1 1 1", "1 1 2 1 1")]
        [DataRow("1 1 1 1 1", "1 1 1 2 1")]
        [DataRow("1 1 1 1 1", "1 1 1 1 2")]
        [DataRow("* * * * *", "1 1 1 1 1")]

        [DataRow("* * 31 1 *", "* * L    1 *")]
        [DataRow("* * L  * *", "* * LW   * *")]
        [DataRow("* * LW * *", "* * L-1W * *")]
        [DataRow("* * *  * 0", "* * L-1W * 0#1")]
        public void Equals_ReturnsFalse_WhenCronExpressionsAreNotEqual(string leftExpression, string rightExpression)
        {
            var leftCronExpression = CronExpression.Parse(leftExpression);
            var rightCronExpression = CronExpression.Parse(rightExpression);

            Assert.IsFalse(leftCronExpression.Equals(rightCronExpression));
            Assert.IsTrue(leftCronExpression != rightCronExpression);
            Assert.IsTrue(leftCronExpression.GetHashCode() != rightCronExpression.GetHashCode());
        }

        #region 

        [TestMethod]

        // Second specified.

        [DataRow("*      * * * * *", "*                * * * * *", CronFormat.IncludeSeconds)]
        [DataRow("0      * * * * *", "0                * * * * *", CronFormat.IncludeSeconds)]
        [DataRow("1,2    * * * * *", "1,2              * * * * *", CronFormat.IncludeSeconds)]
        [DataRow("1-3    * * * * *", "1,2,3            * * * * *", CronFormat.IncludeSeconds)]
        [DataRow("57-3   * * * * *", "0,1,2,3,57,58,59 * * * * *", CronFormat.IncludeSeconds)]
        [DataRow("*/10   * * * * *", "0,10,20,30,40,50 * * * * *", CronFormat.IncludeSeconds)]
        [DataRow("0/10   * * * * *", "0,10,20,30,40,50 * * * * *", CronFormat.IncludeSeconds)]
        [DataRow("0-20/5 * * * * *", "0,5,10,15,20     * * * * *", CronFormat.IncludeSeconds)]

        [DataRow("10,56-3/2 * * * * *", "0,2,10,56,58 * * * * *", CronFormat.IncludeSeconds)]

        // Minute specified.

        [DataRow("*      * * * *", "0 *                * * * *", CronFormat.Standard)]
        [DataRow("0      * * * *", "0 0                * * * *", CronFormat.Standard)]
        [DataRow("1,2    * * * *", "0 1,2              * * * *", CronFormat.Standard)]
        [DataRow("1-3    * * * *", "0 1,2,3            * * * *", CronFormat.Standard)]
        [DataRow("57-3   * * * *", "0 0,1,2,3,57,58,59 * * * *", CronFormat.Standard)]
        [DataRow("*/10   * * * *", "0 0,10,20,30,40,50 * * * *", CronFormat.Standard)]
        [DataRow("0/10   * * * *", "0 0,10,20,30,40,50 * * * *", CronFormat.Standard)]
        [DataRow("0-20/5 * * * *", "0 0,5,10,15,20     * * * *", CronFormat.Standard)]

        [DataRow("10,56-3/2 * * * *", "0 0,2,10,56,58 * * * *", CronFormat.Standard)]

        [DataRow("* *      * * * *", "* *                * * * *", CronFormat.IncludeSeconds)]
        [DataRow("* 0      * * * *", "* 0                * * * *", CronFormat.IncludeSeconds)]
        [DataRow("* 1,2    * * * *", "* 1,2              * * * *", CronFormat.IncludeSeconds)]
        [DataRow("* 1-3    * * * *", "* 1,2,3            * * * *", CronFormat.IncludeSeconds)]
        [DataRow("* 57-3   * * * *", "* 0,1,2,3,57,58,59 * * * *", CronFormat.IncludeSeconds)]
        [DataRow("* */10   * * * *", "* 0,10,20,30,40,50 * * * *", CronFormat.IncludeSeconds)]
        [DataRow("* 0/10   * * * *", "* 0,10,20,30,40,50 * * * *", CronFormat.IncludeSeconds)]
        [DataRow("* 0-20/5 * * * *", "* 0,5,10,15,20     * * * *", CronFormat.IncludeSeconds)]

        [DataRow("* 10,56-3/2 * * * *", "* 0,2,10,56,58 * * * *", CronFormat.IncludeSeconds)]

        // Hour specified.

        [DataRow("* *      * * *", "0 * *             * * *", CronFormat.Standard)]
        [DataRow("* 0      * * *", "0 * 0             * * *", CronFormat.Standard)]
        [DataRow("* 1,2    * * *", "0 * 1,2           * * *", CronFormat.Standard)]
        [DataRow("* 1-3    * * *", "0 * 1,2,3         * * *", CronFormat.Standard)]
        [DataRow("* 22-3   * * *", "0 * 0,1,2,3,22,23 * * *", CronFormat.Standard)]
        [DataRow("* */10   * * *", "0 * 0,10,20       * * *", CronFormat.Standard)]
        [DataRow("* 0/10   * * *", "0 * 0,10,20       * * *", CronFormat.Standard)]
        [DataRow("* 0-20/5 * * *", "0 * 0,5,10,15,20  * * *", CronFormat.Standard)]

        [DataRow("* 10,22-3/2 * * *", "0 * 0,2,10,22    * * *", CronFormat.Standard)]

        [DataRow("* * *      * * *", "* * *             * * *", CronFormat.IncludeSeconds)]
        [DataRow("* * 0      * * *", "* * 0             * * *", CronFormat.IncludeSeconds)]
        [DataRow("* * 1,2    * * *", "* * 1,2           * * *", CronFormat.IncludeSeconds)]
        [DataRow("* * 1-3    * * *", "* * 1,2,3         * * *", CronFormat.IncludeSeconds)]
        [DataRow("* * 22-3   * * *", "* * 0,1,2,3,22,23 * * *", CronFormat.IncludeSeconds)]
        [DataRow("* * */10   * * *", "* * 0,10,20       * * *", CronFormat.IncludeSeconds)]
        [DataRow("* * 0/10   * * *", "* * 0,10,20       * * *", CronFormat.IncludeSeconds)]
        [DataRow("* * 0-20/5 * * *", "* * 0,5,10,15,20  * * *", CronFormat.IncludeSeconds)]

        [DataRow("* * 10,22-3/2 * * *", "* * 0,2,10,22    * * *", CronFormat.IncludeSeconds)]

        // Day specified.

        [DataRow("* * *      * *", "0 * * *           * *", CronFormat.Standard)]
        [DataRow("* * 1      * *", "0 * * 1           * *", CronFormat.Standard)]
        [DataRow("* * 1,2    * *", "0 * * 1,2         * *", CronFormat.Standard)]
        [DataRow("* * 1-3    * *", "0 * * 1,2,3       * *", CronFormat.Standard)]
        [DataRow("* * 30-3   * *", "0 * * 1,2,3,30,31 * *", CronFormat.Standard)]
        [DataRow("* * */10   * *", "0 * * 1,11,21,31  * *", CronFormat.Standard)]
        [DataRow("* * 1/10   * *", "0 * * 1,11,21,31  * *", CronFormat.Standard)]
        [DataRow("* * 1-20/5 * *", "0 * * 1,6,11,16   * *", CronFormat.Standard)]

        [DataRow("* * L     * *", "0 * * L     * *", CronFormat.Standard)]
        [DataRow("* * L-0   * *", "0 * * L     * *", CronFormat.Standard)]
        [DataRow("* * L-10  * *", "0 * * L-10  * *", CronFormat.Standard)]
        [DataRow("* * LW    * *", "0 * * LW    * *", CronFormat.Standard)]
        [DataRow("* * L-0W  * *", "0 * * LW    * *", CronFormat.Standard)]
        [DataRow("* * L-10W * *", "0 * * L-10W * *", CronFormat.Standard)]
        [DataRow("* * 10W   * *", "0 * * 10W   * *", CronFormat.Standard)]

        [DataRow("* * 10,29-3/2 * *", "0 * * 2,10,29,31 * *", CronFormat.Standard)]

        [DataRow("* * * *      * *", "* * * *           * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * 1      * *", "* * * 1           * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * 1,2    * *", "* * * 1,2         * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * 1-3    * *", "* * * 1,2,3       * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * 30-3   * *", "* * * 1,2,3,30,31 * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * */10   * *", "* * * 1,11,21,31  * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * 1/10   * *", "* * * 1,11,21,31  * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * 1-20/5 * *", "* * * 1,6,11,16   * *", CronFormat.IncludeSeconds)]

        [DataRow("* * * L     * *", "* * * L     * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * L-0   * *", "* * * L     * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * L-10  * *", "* * * L-10  * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * LW    * *", "* * * LW    * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * L-0W  * *", "* * * LW    * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * L-10W * *", "* * * L-10W * *", CronFormat.IncludeSeconds)]
        [DataRow("* * * 10W   * *", "* * * 10W   * *", CronFormat.IncludeSeconds)]

        [DataRow("* * * 10,29-3/2 * *", "* * * 2,10,29,31 * *", CronFormat.IncludeSeconds)]

        // Month specified.

        [DataRow("* * * *      *", "0 * * * *           *", CronFormat.Standard)]
        [DataRow("* * * 1      *", "0 * * * 1           *", CronFormat.Standard)]
        [DataRow("* * * 1,2    *", "0 * * * 1,2         *", CronFormat.Standard)]
        [DataRow("* * * 1-3    *", "0 * * * 1,2,3       *", CronFormat.Standard)]
        [DataRow("* * * 11-3   *", "0 * * * 1,2,3,11,12 *", CronFormat.Standard)]
        [DataRow("* * * */10   *", "0 * * * 1,11        *", CronFormat.Standard)]
        [DataRow("* * * 1/10   *", "0 * * * 1,11        *", CronFormat.Standard)]
        [DataRow("* * * 1-12/5 *", "0 * * * 1,6,11      *", CronFormat.Standard)]

        [DataRow("* * * 10,11-3/2 *", "0 * * * 1,3,10,11 *", CronFormat.Standard)]

        [DataRow("* * * * *      *", "* * * * *           *", CronFormat.IncludeSeconds)]
        [DataRow("* * * * 1      *", "* * * * 1           *", CronFormat.IncludeSeconds)]
        [DataRow("* * * * 1,2    *", "* * * * 1,2         *", CronFormat.IncludeSeconds)]
        [DataRow("* * * * 1-3    *", "* * * * 1,2,3       *", CronFormat.IncludeSeconds)]
        [DataRow("* * * * 11-3   *", "* * * * 1,2,3,11,12 *", CronFormat.IncludeSeconds)]
        [DataRow("* * * * */10   *", "* * * * 1,11        *", CronFormat.IncludeSeconds)]
        [DataRow("* * * * 1/10   *", "* * * * 1,11        *", CronFormat.IncludeSeconds)]
        [DataRow("* * * * 1-12/5 *", "* * * * 1,6,11      *", CronFormat.IncludeSeconds)]

        [DataRow("* * * * 10,11-3/2 *", "* * * * 1,3,10,11 *", CronFormat.IncludeSeconds)]

        // Day of week specified.

        [DataRow("* * * * *    ", "0 * * * * *      ", CronFormat.Standard)]
        [DataRow("* * * * MON  ", "0 * * * * 1      ", CronFormat.Standard)]
        [DataRow("* * * * 1    ", "0 * * * * 1      ", CronFormat.Standard)]
        [DataRow("* * * * 1,2  ", "0 * * * * 1,2    ", CronFormat.Standard)]
        [DataRow("* * * * 1-3  ", "0 * * * * 1,2,3  ", CronFormat.Standard)]
        [DataRow("* * * * 6-1  ", "0 * * * * 0,1,6  ", CronFormat.Standard)]
        [DataRow("* * * * */2  ", "0 * * * * 0,2,4,6", CronFormat.Standard)]
        [DataRow("* * * * 0/2  ", "0 * * * * 0,2,4,6", CronFormat.Standard)]
        [DataRow("* * * * 1-6/5", "0 * * * * 1,6    ", CronFormat.Standard)]

        [DataRow("* * * * 0L ", "0 * * * * 0L ", CronFormat.Standard)]
        [DataRow("* * * * 5#1", "0 * * * * 5#1", CronFormat.Standard)]

        // ReSharper disable once StringLiteralTypo
        [DataRow("* * * * SUNL ", "0 * * * * 0L ", CronFormat.Standard)]
        [DataRow("* * * * FRI#1", "0 * * * * 5#1", CronFormat.Standard)]

        [DataRow("* * * * 3,6-2/3", "0 * * * * 2,3,6", CronFormat.Standard)]

        [DataRow("* * * * * *    ", "* * * * * *      ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * MON  ", "* * * * * 1      ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * 1    ", "* * * * * 1      ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * 1,2  ", "* * * * * 1,2    ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * 1-3  ", "* * * * * 1,2,3  ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * 6-1  ", "* * * * * 0,1,6  ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * */2  ", "* * * * * 0,2,4,6", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * 0/2  ", "* * * * * 0,2,4,6", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * 1-6/5", "* * * * * 1,6    ", CronFormat.IncludeSeconds)]

        [DataRow("* * * * * 0L ", "* * * * * 0L ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * 5#1", "* * * * * 5#1", CronFormat.IncludeSeconds)]

        // ReSharper disable once StringLiteralTypo
        [DataRow("* * * * * SUNL ", "* * * * * 0L ", CronFormat.IncludeSeconds)]
        [DataRow("* * * * * FRI#1", "* * * * * 5#1", CronFormat.IncludeSeconds)]

        [DataRow("* * * * * 3,6-2/3", "* * * * * 2,3,6", CronFormat.IncludeSeconds)]
        public void ToString_ReturnsCorrectString(string cronExpression, string expectedResult, CronFormat format)
        {
            var expression = CronExpression.Parse(cronExpression, format);

            // remove redundant spaces.
            var expectedString = Regex.Replace(expectedResult, @"\s+", " ").Trim();

            Assert.AreEqual(expectedString, expression.ToString());
        }

        public static IEnumerable<object[]> GetTimeZones()
        {
            yield return new object[] { EasternTimeZone };
            yield return new object[] { JordanTimeZone };
            yield return new object[] { TimeZoneInfo.Utc };
        }
        #endregion

        private static DateTimeOffset GetInstantFromLocalTime(string localDateTimeString, TimeZoneInfo zone)
        {
            localDateTimeString = localDateTimeString.Trim();

            var dateTime = DateTime.ParseExact(
                localDateTimeString,
                new[]
                {
                            "HH:mm:ss",
                            "HH:mm",
                            "yyyy-MM-dd HH:mm:ss",
                            "yyyy-MM-dd HH:mm",
                            "yyyy-MM-dd"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.NoCurrentDateDefault);

            var localDateTime = new DateTime(
                dateTime.Year != 1 ? dateTime.Year : Today.Year,
                dateTime.Year != 1 ? dateTime.Month : Today.Month,
                dateTime.Year != 1 ? dateTime.Day : Today.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second);

            return new DateTimeOffset(localDateTime, zone.GetUtcOffset(localDateTime));
        }

        private static DateTimeOffset GetInstant(string dateTimeOffsetString)
        {
            dateTimeOffsetString = dateTimeOffsetString.Trim();

            var dateTime = DateTimeOffset.ParseExact(
                dateTimeOffsetString,
                new[]
                {
                            "yyyy-MM-dd HH:mm:ss zzz",
                            "yyyy-MM-dd HH:mm zzz",
                            "yyyy-MM-dd HH:mm:ss.fffffff zzz"
                },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None);

            return dateTime;
        }
    }
}
