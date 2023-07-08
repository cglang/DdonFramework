using System;
using Ddon.Core.Services.IdWorker.Guids;
using Microsoft.Extensions.Options;

namespace Ddon.Core.Services.IdWorker.Snowflake
{
    /**
     * 原始代码
     * https://github.com/imadcn/idworker/blob/master/src/main/java/com/imadcn/framework/idworker/algorithm/Snowflake.java
     * 
     * Snowflake的结构如下(每部分用-分开):
     * 0 - 0000000000 0000000000 0000000000 0000000000 0 - 00000 - 00000 - 000000000000
     * - 1位标识，由于long基本类型在Java中是带符号的，最高位是符号位，正数是0，负数是1，所以id一般是正数，最高位是0
     * - 41位时间戳(毫秒级)，注意，41位时间戳不是存储当前时间的时间戳，而是存储时间戳的差值（当前时间戳 - 开始时间戳)得到的值），
     *   这里的的开始时间戳，一般是我们的id生成器开始使用的时间，由我们程序来指定的（如下下面程序epoch属性）。41位的时间戳，可以使用69年
     * - 10位的数据机器位，可以部署在1024个节点，包括5位datacenterId和5位workerId
     * - 12位序列，毫秒内的计数，12位的计数顺序号支持每个节点每毫秒(同一机器，同一时间戳)产生4096个ID序号 加起来刚好64位，为一个Long型。
     * 
     * SnowFlake的优点是，整体上按照时间自增排序，并且整个分布式系统内不会产生ID碰撞(由数据中心ID和机器ID作区分)，并且效率较高，经测试，SnowFlake每秒能够产生26万ID左右。
     * 
     * 注意这里进行了小改动:
     * - Snowflake是5位的datacenter加5位的机器id; 这里变成使用10位的机器id (b)
     * - 对系统时间的依赖性非常强，需关闭ntp的时间同步功能。当检测到ntp时间调整后，将会拒绝分配id
     */

    /// <summary>
    /// 雪花算法
    /// </summary>
    public class SnowflakeGenerator : ISnowflakeGenerator
    {
        /// <summary>
        /// 机器ID
        /// </summary>
        private readonly uint workerId;

        /// <summary>
        /// 时间起始标记点，作为基准，一般取系统的最近时间，默认2017-01-01
        /// </summary>
        private const long Epoch = 1483200000000L;

        /// <summary>
        /// 机器id所占的位数（源设计为5位，这里取消dataCenterId，采用10位，既1024台）
        /// </summary>
        private const int WorkerIdBits = 10;

        /// <summary>
        /// 机器ID最大值: 1023 (从0开始)
        /// </summary>
        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);

        /// <summary>
        /// 序列在id中占的位数
        /// </summary>
        private const int SequenceBits = 12;

        /// <summary>
        /// 生成序列的掩码，这里为4095 (0b111111111111=0xfff=4095)，12位
        /// </summary>
        private const long SequenceMask = -1L ^ (-1L << SequenceBits);

        /// <summary>
        /// 机器ID向左移12位
        /// </summary>
        private const int WorkerIdShift = SequenceBits;

        /// <summary>
        /// 时间戳向左移22位(5+5+12)
        /// </summary>
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits;

        /// <summary>
        /// 并发控制，毫秒内序列(0~4095)
        /// </summary>
        private long sequence = 0;

        /// <summary>
        /// 上次生成ID的时间戳
        /// </summary>
        private long lastTimestamp = -1L;

        /// <summary>
        /// 100,000
        /// </summary>
        private const int HUNDRED_K = 100_000;

        /// <summary>
        /// sequence随机种子（兼容低并发下，sequence均为0的情况）
        /// </summary>
        private static readonly Random RANDOM = new();

        /// <summary>
        /// CTOR
        /// </summary>
        public SnowflakeGenerator(IOptions<SnowflakeGeneratorOptions> snowflakeOptions)
        {
            workerId = snowflakeOptions.Value.GetDefaultWorkerId();
        }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="workerId">机器Id</param>
        private SnowflakeGenerator(uint workerId)
        {
            this.workerId = workerId;
        }

        /// <summary>
        /// SnowflakeGenerator Builder
        /// </summary>
        /// <param name="workerId">机器Id</param>
        /// <returns></returns>
        public static SnowflakeGenerator Init(uint workerId = SnowflakeGeneratorOptions.DefaultWorkerId)
        {
            return new SnowflakeGenerator(workerId);
        }

        /// <summary>
        /// 批量获取ID
        /// </summary>
        /// <param name="size">获取数量，最多10万个</param>
        /// <returns></returns>
        public long[] NextId(uint size)
        {
            long[] ids = new long[size];
            for (int i = 0; i < size; i++)
            {
                ids[i] = NextId();
            }
            return ids;
        }

        /// <summary>
        /// 获得Id
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long NextId()
        {
            lock (this)
            {
                long timestamp = TimeGen();
                // 如果上一个timestamp与新产生的相等，则sequence加一(0-4095循环);
                if (lastTimestamp == timestamp)
                {
                    // 对新的timestamp，sequence从0开始
                    sequence = sequence + 1 & SequenceMask;
                    // 毫秒内序列溢出
                    if (sequence == 0)
                    {
                        // 阻塞到下一个毫秒,获得新的时间戳
                        sequence = RANDOM.Next(4095);
                        timestamp = TilNextMillis(lastTimestamp);
                    }
                }
                else
                {
                    // 时间戳改变，毫秒内序列重置
                    sequence = RANDOM.NextInt64(4095);
                }
                // 如果当前时间小于上一次ID生成的时间戳，说明系统时钟回退过这个时候应当抛出异常
                if (timestamp < lastTimestamp)
                {
                    throw new Exception($"Clock moved backwards. Refusing to generate id for {timestamp} milliseconds.");
                }
                lastTimestamp = timestamp;
                // 移位并通过或运算拼到一起组成64位的ID
                return timestamp - Epoch << TimestampLeftShift | workerId << WorkerIdShift | sequence;
            }
        }

        /// <summary>
        /// 等待下一个毫秒的到来, 保证返回的毫秒数在参数lastTimestamp之后
        /// </summary>
        /// <param name="lastTimestamp">上次生成ID的时间戳</param>
        /// <returns>下一个毫秒</returns>
        private static long TilNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }


        /// <summary>
        /// 获得系统当前毫秒数
        /// </summary>
        /// <returns>系统当前毫秒数</returns>
        private static long TimeGen()
        {
            return DateTime.UtcNow.Ticks;
        }
    }
}
