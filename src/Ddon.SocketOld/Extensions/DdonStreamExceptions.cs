using Ddon.Socket.Extra;
using System.Text;

namespace Ddon.Socket.Extensions
{
    public static class DdonStreamExceptions
    {
        /// <summary>
        /// 从流中读取指定长度bytes
        /// </summary>
        /// <param name="stream">要读取的流</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static byte[] ReadByte(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            if (length > DdonSocketConst.Count)
            {
                // TODO 这里需要测试
                int index = 0;
                while (length != 0)
                {
                    index += stream.Read(bytes, index, DdonSocketConst.Count);
                    length -= bytes.Length;
                }
                return bytes;
            }
            else
            {
                stream.Read(bytes, 0, length);
                return DdonSocketCommon.ByteCut(bytes);
            }
        }

        /// <summary>
        /// 从流中读取指定长度bytes
        /// </summary>
        /// <param name="stream">要读取的流</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static async Task<byte[]> ReadByteAsync(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            if (length > DdonSocketConst.Count)
            {
                // TODO 这里需要测试
                int index = 0;
                while (length != 0)
                {
                    index += await stream.ReadAsync(bytes.AsMemory(index, DdonSocketConst.Count));
                    length -= bytes.Length;
                }
                return bytes;
            }
            else
            {
                await stream.ReadAsync(bytes.AsMemory(0, length));
                return DdonSocketCommon.ByteCut(bytes);
            }
        }

        /// <summary>
        /// 从流中读取指定长度byte的字符串
        /// </summary>
        /// <param name="stream">要读取的流</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static async Task<string> ReadStringAsync(this Stream stream, int length)
        {
            byte[] bytes = await stream.ReadByteAsync(length);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 向流中写入文本
        /// </summary>
        /// <param name="stream">要写入的流</param>
        /// <param name="content">文本内容</param>
        /// <returns></returns>
        public static async Task SendStringAsync(this Stream stream, string content)
        {
            var dataBytes = Encoding.UTF8.GetBytes(content);
            await stream.WriteAsync(dataBytes);
        }
    }
}
