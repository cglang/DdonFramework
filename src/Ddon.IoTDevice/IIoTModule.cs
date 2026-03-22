namespace Ddon.IoTDevice
{
    public interface IIoTModule
    {
        /// <summary>
        /// 设备
        /// </summary>
        IIotDevice IotDevice { get; set; }
    }
}
