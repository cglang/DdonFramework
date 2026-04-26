namespace Ddon.UniPLC.Models;

/// <summary>
/// PLC 地址统一模型
/// </summary>
public class PlcAddress
{
    /// <summary>
    /// 地址区域（如 DB, M, I, Q, D 等）
    /// </summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// 块号（主要用于 Siemens DB 块）
    /// </summary>
    public int BlockNumber { get; set; }

    /// <summary>
    /// 字节偏移量
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// 位偏移量
    /// </summary>
    public int Bit { get; set; } = -1;

    /// <summary>
    /// 数据类型
    /// </summary>
    public PlcDataType DataType { get; set; } = PlcDataType.Byte;

    /// <summary>
    /// 原始地址字符串
    /// </summary>
    public string RawAddress { get; set; } = string.Empty;

    /// <summary>
    /// 获取规范化的地址字符串
    /// </summary>
    public override string ToString()
    {
        if (Bit >= 0)
            return $"{Area}{BlockNumber}.{Offset}.{Bit}";
        return $"{Area}{BlockNumber}.{Offset}";
    }
}

/// <summary>
/// PLC 数据类型枚举
/// </summary>
public enum PlcDataType
{
    Bool,
    Byte,
    Short,
    UShort,
    Int,
    UInt,
    Long,
    ULong,
    Float,
    Double,
    String,
    DateTime,
    Struct,
    Array,
    Enum
}
