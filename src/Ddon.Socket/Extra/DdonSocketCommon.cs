namespace DdonSocket.Extra
{
    public class DdonSocketCommon
    {
        public static byte[] GetHeadBytes(int code, DdonSocketDataType type, int length, Guid clientId, Guid sendClientId = default, Guid sendGroupId = default)
        {
            DdonSocketHeadDto head = new()
            {
                Opcode = code,
                Type = type,
                Length = length,
                ClientId = clientId,
                SendClientId = sendClientId,
                SendGroup = sendGroupId
            };

            return head.GetBytes();
        }


        public static byte[] MergeArrays(byte[] array1, byte[] array2)
        {
            byte[] contentBytes = new byte[DdonSocketConst.HeadLength + array2.Length];
            Array.Copy(array1, contentBytes, array1.Length);
            Array.Copy(array2, 0, contentBytes, DdonSocketConst.HeadLength, array2.Length);
            return contentBytes;
        }

        // <summary>
        /// 去掉byte[] 中特定的byte
        /// </summary>
        /// <param name="b"> 需要处理的byte[]</param>
        /// <param name="cut">byte[] 中需要除去的特定 byte (此处: byte cut = 0x00 ;) </param>
        /// <returns> 返回处理完毕的byte[] </returns>
        public static byte[] ByteCut(byte[] b, byte cut = 0x00)
        {
            List<byte> list = new(b);
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
