using System;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC;

public static class PlcByteSize
{
    public static int Get(PlcDataType type, int stringLength = 0) => type switch
    {
        PlcDataType.Bool => 1,
        PlcDataType.Byte => 1,
        PlcDataType.Int16 => 2,
        PlcDataType.UInt16 => 2,
        PlcDataType.Int32 => 4,
        PlcDataType.UInt32 => 4,
        PlcDataType.Float => 4,
        PlcDataType.Double => 8,
        PlcDataType.String => stringLength > 0 ? stringLength : 256,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
