using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        private const int BUFFER_SIZE = 4096;

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
        /// <param name="data">数据</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static void ReadLengthBytes(this Stream stream, out byte[] data, int length)
        {
            data = new byte[length];
            var bufferSize = length < BUFFER_SIZE ? length : BUFFER_SIZE;
            var buffer = new byte[bufferSize];
            var index = 0;
            while (index != length)
            {
                var readLength = stream.Read(buffer, 0, bufferSize);
                Array.Copy(buffer, 0, data, index, readLength);
                index += readLength;
            }
        }

        /// <summary>
        /// 从流中读取指定长度bytes
        /// </summary>
        /// <param name="stream">要读取的流</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static async Task<byte[]> ReadLengthBytesAsync(this Stream stream, int length)
        {
            var data = new byte[length];
            var bufferSize = length < BUFFER_SIZE ? length : BUFFER_SIZE;
            var buffer = new byte[bufferSize];
            var index = 0;
            while (index != length)
            {
                var readLength = await stream.ReadAsync(buffer.AsMemory(0, bufferSize));
                Array.Copy(buffer, 0, data, index, readLength);
                index += readLength;
            }
            return data;
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

    }
}