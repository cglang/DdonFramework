namespace DdonSocket.Extra
{
    public struct DdonSocketOpcode
    {
        /// <summary>
        /// 认证
        /// </summary>
        public static int Authentication => 10001;

        /// <summary>
        /// 向其他客户端转发数据
        /// </summary>
        public static int Repost => 10002;

        /// <summary>
        /// 获取客户端列表
        /// </summary>
        public static int Clients => 10003;
    }
}
