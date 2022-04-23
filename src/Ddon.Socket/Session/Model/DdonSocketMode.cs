namespace Ddon.Socket.Session.Model
{
    /// <summary>
    /// 数据传输模式
    /// </summary>
    public enum DdonSocketMode
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
}
