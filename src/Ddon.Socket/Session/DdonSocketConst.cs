namespace Ddon.Socket.Session
{
    public class DdonSocketConst
    {
        /// <summary>
        /// 发送头长度
        /// </summary>
        public const int HeadLength = 500;

        /// <summary>
        /// 每次接收/发送文件的长度
        /// </summary>
        public const int FileLength = 1024 * 1024 * 10;    //100 KByte = 0.1 MByte
    }
}
