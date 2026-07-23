namespace Ddon.VitrinPLC.Models
{
    public sealed class MemoryRegionInfo
    {
        public string RegionKey { get; init; }
        public string Area { get; init; }
        public int StartOffset { get; init; }
        public int Length { get; init; }
    }
}
