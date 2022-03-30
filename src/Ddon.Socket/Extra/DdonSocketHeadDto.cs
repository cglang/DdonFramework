using System.Text;
using System.Text.Json;

namespace Ddon.Socket.Extra
{
    public class DdonSocketHeadDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 模式
        /// </summary>
        public Mode Mode { get; set; }

        /// <summary>
        /// 响应码
        /// </summary>
        public ResponseCode Code { get; set; }

        /// <summary>
        /// 传输的数据类型
        /// </summary>
        public DdonSocketDataType DataType { get; set; }

        /// <summary>
        /// 操作码
        /// </summary>
        public int OpCode { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length { get; set; }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    /// <summary>
    /// 数据传输模式
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// 发送模式
        /// </summary>
        Normal = 1,
        /// <summary>
        /// 请求模式
        /// </summary>
        Request = 2,
        /// <summary>
        /// 响应模式
        /// </summary>
        Response = 3
    }

    /// <summary>
    /// 传输的数据类型
    /// </summary>
    public enum DdonSocketDataType
    {
        /// <summary>
        /// 文件
        /// </summary>
        File,
        /// <summary>
        /// 文本
        /// </summary>
        String,
        /// <summary>
        /// 大文件流
        /// </summary>
        Byte
    }

    /// <summary>
    /// 响应码
    /// </summary>
    public enum ResponseCode
    {
        OK = 200,
        Error = 500
    }
}
