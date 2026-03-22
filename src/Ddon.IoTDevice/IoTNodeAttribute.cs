using System;

namespace Ddon.IoTDevice
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IoTNodeAttribute : Attribute
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// 是否是批量读取
        /// </summary>
        public bool IsBatchRead { get; set; }

        public IoTNodeAttribute(string address, bool isBatchRead = false)
        {
            Address = address;
            IsBatchRead = isBatchRead;
        }
    }
}
