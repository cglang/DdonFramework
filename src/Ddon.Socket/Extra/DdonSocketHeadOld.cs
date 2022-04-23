using System;
using System.Text;
using System.Text.Json;

namespace Ddon.ConvenientSocket.Extra
{
    public class DdonSocketHeadOld
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
        /// 操作码
        /// </summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length { get; set; }

        public static byte[] GetHeadBytes(Mode mode, int length, string? router, Guid id = default, ResponseCode code = ResponseCode.OK)
        {
            DdonSocketHeadOld head = new()
            {
                Id = id,
                Code = code,
                Route = router ?? string.Empty,
                Mode = mode,
                Length = length
            };

            return head.GetBytes();
        }

        public DdonSocketHeadOld Response(int length, ResponseCode code = ResponseCode.OK)
        {
            Length = length;
            Mode = Mode.Response;
            Route = string.Empty;
            Code = code;

            return this;
        }

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
        /// 文本模式
        /// </summary>
        String,
        /// <summary>
        /// 文件模式
        /// </summary>
        File,
        /// <summary>
        /// Byte流模式
        /// </summary>
        Byte,
        /// <summary>
        /// 请求模式
        /// </summary>
        Request,
        /// <summary>
        /// 响应模式
        /// </summary>
        Response,

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
