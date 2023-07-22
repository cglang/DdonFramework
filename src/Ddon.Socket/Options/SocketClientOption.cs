using System.Diagnostics.CodeAnalysis;

namespace Ddon.Socket.Options
{
    public class SocketClientOption
    {
        /// <summary>
        /// 服务端地址
        /// </summary>
        [AllowNull]
        public string Host { get; set; }

        /// <summary>
        /// 服务端端口
        /// </summary>
        public int Port { get; set; }

        ///<summary>
        /// 断开连接是否自动重连
        /// </summary>
        public bool IsReconnection { get; set; }
    }
}
