namespace Ddon.IoTDevice
{
    public interface IIoTNode<T>
    {
        IIoTModule Module { get; }

        /// <summary>
        /// 地址
        /// </summary>
        string Address { get; }

        /// <summary>
        /// 原始节点值
        /// </summary>
        byte[] OriginalValue { get; set; }

        /// <summary>
        /// 节点值
        /// </summary>
        T Value { get; set; }
    }
}
