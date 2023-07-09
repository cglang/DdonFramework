using System.Collections;
using Ddon.Core.Services.IdWorker.Snowflake;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Core.Tests
{
    [TestClass]
    public class SnowflakeTests
    {
        [TestMethod]
        public void IdGeneratorTest()
        {
            var snowflake = SnowflakeGenerator.Init(1);

            for (int i = 0; i < 100; i++)
            {
                var ids = snowflake.NextId(100000);
                Assert.IsFalse(IsRepeat(ref ids));
            }
        }

        /// <summary>
        /// Hashtable 方法
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsRepeat(ref long[] array)
        {
            Hashtable ht = new();
            for (int i = 0; i < array.Length; i++)
            {
                if (ht.Contains(array[i]))
                {
                    return true;
                }
                else
                {
                    ht.Add(array[i], array[i]);
                }
            }
            return false;
        }
    }
}
