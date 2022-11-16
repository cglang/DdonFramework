using System.Net.Sockets;
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
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public static async Task<Memory<byte>> ReadLengthAsync(this Stream stream, int length)
        {
            if (length <= 0) throw new ArgumentException("参数 length 必须大于 0");

            var data = new byte[length].AsMemory();

            var index = 0;
            var buffsize = BUFFER_SIZE;
            do
            {
                if (index + buffsize > length) buffsize = length - index;
                int readLength = await stream.ReadAsync(data.Slice(index, buffsize));
                index += readLength;

                if (readLength == 0) throw new SocketException();
            }
            while (index < length);

            return data;
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

    }
}