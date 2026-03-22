using IoTClient;

namespace Ddon.IoTDevice.IoTClient
{
    /// <summary>
    /// IIoTClient 接口
    /// </summary>
    public interface IIoTClient
    {
        /// <summary>
        /// 版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 是否是连接的
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 警告日志委托
        /// 为了可用性，会对异常网络进行重试。此类日志通过委托接口给出去。
        /// </summary>
        LoggerDelegate WarningLog { get; set; }

        /// <summary>
        /// 打开连接（如果已经是连接状态会先关闭再打开）
        /// </summary>
        /// <returns></returns>
        Result Open();

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        Result Close();

        /// <summary>
        /// 读取Bytes
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        Result<byte[]> ReadBytes(string address, ushort length, bool isBit = false);

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        Result WriteBytes(string address, byte[] value);
    }
}

