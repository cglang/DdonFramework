using System;
using System.Collections.Generic;

namespace Ddon.Core.Use
{
    public static class DdonArray
    {
        /// <summary>
        /// 合并两个byte[]，
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <param name="array1Length">指定多少数组之后再合并array2，中间使用0补位</param>
        /// <returns></returns>
        public static byte[] MergeArrays(byte[] array1, byte[] array2, int array1Length = default)
        {
            if (array1Length == default) array1Length = array1.Length;

            byte[] contentBytes = new byte[array1Length + array2.Length];
            Array.Copy(array1, contentBytes, array1.Length);
            Array.Copy(array2, 0, contentBytes, array1Length, array2.Length);
            return contentBytes;
        }

        /// <summary>
        /// 去掉byte[] 中特定的byte
        /// </summary>
        /// <param name="bytes"> 需要处理的byte[]</param>
        /// <param name="cut">byte[] 中需要除去的特定 byte (此处: byte cut = 0x00 ;) </param>
        /// <returns> 返回处理完毕的byte[] </returns>
        public static byte[] ByteCut(IEnumerable<byte> bytes, byte cut = 0x00)
        {
            List<byte> list = new(bytes);
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == cut)
                    list.RemoveAt(i);
            }
            var lastbyte = new byte[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                lastbyte[i] = list[i];
            }
            return lastbyte;
        }
    }
}
