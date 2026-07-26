namespace Ddon.VitrinPLC.Models;

public sealed class ParsedAddress
{
    public string Original { get; init; }
    public string RegionKey { get; init; }
    public string Area { get; init; }
    public int ByteOffset { get; init; }
    public int BitIndex { get; init; }
    public PlcDataType DataType { get; init; }
    public bool IsBit { get; init; }
}
