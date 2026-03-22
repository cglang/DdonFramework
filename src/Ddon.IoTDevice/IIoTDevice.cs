using Ddon.IoTDevice.IoTClient;

/*
 * 1. 使用IoTClient作为基础库，用于实现对不同PLC的连接实现
https://github.com/zhaopeiym/IoTClient/blob/master/README-zh_CN.md
2. 对不同的PLC进行抽象，使用统一的方式进行读写，使访问不同的硬件设备方式都统一起来
3. 对设备层进行抽象，打造一套统一的可拓展的驱动模型，访问点位就像是访问内存对象一样方便
 */
namespace Ddon.IoTDevice
{
    /// <summary>
    /// 设备
    /// </summary>
    public interface IIotDevice
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        IIoTClient Client { get; }
    }
}
