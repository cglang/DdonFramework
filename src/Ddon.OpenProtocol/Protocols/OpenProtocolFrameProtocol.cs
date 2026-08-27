using System;
using System.Text;

namespace Ddon.OpenProtocol.Protocols
{
    /// <summary>
    /// Open Protocol 帧切分协议。
    /// 数据流格式：<c>[帧][\0][帧][\0]...</c>，每一帧以 NUL（\0）终止。
    /// 切分以 NUL 终止符为准，不依赖头部长度字段——
    /// 因为不同实现（含 OpenProtocolInterpreter 库）输出的长度字段可能与实际字符数不一致。
    /// </summary>
    public class OpenProtocolFrameProtocol
    {
        private const int MinimumFrameLength = 20;

        private const int MaximumFrameLength = 65535;

        public byte[] Encode(object message)
        {
            if (message is byte[] data)
                return data;

            if (message is string text)
                return Encoding.ASCII.GetBytes(text);

            throw new InvalidOperationException("OpenProtocolFrameProtocol 仅支持 byte[] 或 string 负载。");
        }

        /// <summary>
        /// 从缓冲中切出一帧。返回 <paramref name="Frame"/> 表示已得到完整一帧，
        /// <paramref name="Consumed"/> 为消费的字节数（含帧后 NUL）。
        /// 返回 null 表示数据不完整（等待更多数据）或数据为噪声（已跳过部分字节）。
        /// </summary>
        public (byte[]? Frame, int Consumed) Decode(byte[] buffer, int offset, int count)
        {
            int pos = offset;
            int end = offset + count;

            // 跳过杂散字节（上一个 NUL 残留、垃圾等），直到帧头（数字开头）
            while (pos < end && !IsDigit(buffer[pos]))
                pos++;

            int dropped = pos - offset;

            // 查找 NUL 终止符
            int nul = pos;
            while (nul < end && buffer[nul] != 0)
                nul++;

            if (nul >= end)
            {
                // NUL 未到：帧不完整，等待更多数据
                return (null, dropped);
            }

            int frameLength = nul - pos;

            if (frameLength < MinimumFrameLength || frameLength > MaximumFrameLength)
            {
                // 帧长非法：丢弃到 NUL（含 NUL），继续解析后续数据
                return (null, dropped + frameLength + 1);
            }

            var frame = new byte[frameLength];
            Array.Copy(buffer, pos, frame, 0, frameLength);
            return (frame, dropped + frameLength + 1);
        }

        private static bool IsDigit(byte value) => value >= (byte)'0' && value <= (byte)'9';
    }
}
