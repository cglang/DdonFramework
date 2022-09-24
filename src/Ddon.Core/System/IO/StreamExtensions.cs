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
            var bytes = new byte[length];
            _ = await stream.ReadAsync(bytes.AsMemory(0, length));
            return bytes;
        }

        public static async Task<byte[]> ReadAllBytesAsync(this Stream stream,
            CancellationToken cancellationToken = default)
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
    }
}