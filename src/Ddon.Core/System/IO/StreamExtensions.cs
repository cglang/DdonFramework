using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// 从流中读取指定长度bytes
        /// </summary>
        /// <param name="stream">要读取的流</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static async Task<byte[]> ReadLengthBytesAsync(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            await stream.ReadAsync(bytes.AsMemory(0, length));
            return ByteCut(bytes);
        }

        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            await stream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }

        public static Task CopyToAsync(this Stream stream, Stream destination, CancellationToken cancellationToken)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            return stream.CopyToAsync(
                destination,
                81920, //this is already the default value, but needed to set to be able to pass the cancellationToken
                cancellationToken
            );
        }

        ///// <summary>
        ///// 从流中读取指定长度byte的字符串
        ///// </summary>
        ///// <param name="stream">要读取的流</param>
        ///// <param name="length">读取长度</param>
        ///// <returns></returns>
        //public static async Task<string> ReadStringAsync(this Stream stream, int length)
        //{
        //    byte[] bytes = await stream.ReadLengthBytesAsync(length);
        //    return Encoding.UTF8.GetString(bytes);
        //}

        /// <summary>
        /// 向流中写入文本
        /// </summary>
        /// <param name="stream">要写入的流</param>
        /// <param name="content">文本内容</param>
        /// <returns></returns>
        public static async Task WriteStringAsync(this Stream stream, string content)
        {
            var dataBytes = Encoding.UTF8.GetBytes(content);
            await stream.WriteAsync(dataBytes);
        }

        /// <summary>
        /// 去掉byte[] 中特定的byte
        /// </summary>
        /// <param name="bytes"> 需要处理的byte[]</param>
        /// <param name="cut">byte[] 中需要除去的特定 byte (此处: byte cut = 0x00 ;) </param>
        /// <returns> 返回处理完毕的byte[] </returns>
        private static byte[] ByteCut(byte[] bytes, byte cut = 0x00)
        {
            List<byte> list = new(bytes);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == cut)
                    list.RemoveAt(i);
            }
            byte[] lastbyte = new byte[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                lastbyte[i] = list[i];
            }
            return lastbyte;
        }
    }
}
