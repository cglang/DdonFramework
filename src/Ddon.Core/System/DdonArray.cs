namespace Ddon.TuuTools.System;

public static class DdonArray
{
    /// <summary>
    /// 合并多个byte[]，
    /// </summary>
    /// <param name="arrays"></param>
    /// <returns></returns>
    public static void MergeArrays(out byte[] contentBytes, params byte[][] arrays)
    {
        var arrayLength = arrays.Sum(array => array.Length);

        contentBytes = new byte[arrayLength];

        var startIndex = 0;
        foreach (var array in arrays)
        {
            Array.Copy(array, 0, contentBytes, startIndex, array.Length);
            startIndex += array.Length;
        }
    }

    /// <summary>
    /// 合并两个byte[]
    /// </summary>
    /// <param name="array1"></param>
    /// <param name="array2"></param>
    /// <param name="array1Length">指定多少数组之后再合并array2，中间使用0补位</param>
    /// <returns></returns>
    public static void MergeArrays(out byte[] contentBytes, byte[] array1, byte[] array2, int array1Length = default)
    {
        if (array1Length == default) array1Length = array1.Length;

        contentBytes = new byte[array1Length + array2.Length];
        Array.Copy(array1, contentBytes, array1.Length);
        Array.Copy(array2, 0, contentBytes, array1Length, array2.Length);
    }

    /// <summary>
    /// 去掉byte[] 中特定的byte
    /// </summary>
    /// <param name="bytes"> 需要处理的byte[]</param>
    /// <param name="cut">byte[] 中需要除去的特定 byte (此处: byte cut = 0x00 ;) </param>
    /// <returns> 返回处理完毕的byte[] </returns>
    public static byte[] ByteCut(Span<byte> bytes, byte cut = 0x00)
    {
        // TODO : 优化
        List<byte> list = new(bytes.ToArray());
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
