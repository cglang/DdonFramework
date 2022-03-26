using System.Text;
using System.Text.Json;

namespace DdonSocket.Extra
{
    public class DdonSocketHeadDto
    {
        /// <summary>
        /// 操作码
        /// </summary>
        public int Opcode { get; set; }

        /// <summary>
        /// 模式
        /// </summary>
        public Mode Mode { get; set; }

        /// <summary>
        /// 传输的数据类型
        /// </summary>
        public DdonSocketDataType Type { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 客户端Id
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// 发送到指定客户
        /// </summary>
        public Guid SendClientId { get; set; }

        /// <summary>
        /// 发送到组
        /// </summary>
        public Guid SendGroup { get; set; }

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
        Send,
        /// <summary>
        /// 请求响应模式
        /// </summary>
        RequestResponse
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
}
