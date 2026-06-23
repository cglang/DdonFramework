using System.Collections.Generic;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC
{
    /// <summary>
    /// 单个 PLC 主机配置：Tags、Regions、扫描间隔。
    /// 不包含协议/连接参数（由 <see cref="VitrinPlcBuilder"/> 的 Add* 方法承载）。
    /// </summary>
    public sealed class PlcHostOptions
    {
        public int ScanInterval { get; set; } = 200;

        internal List<TagDefinition> Tags { get; } = new();

        internal List<RegionConfig> Regions { get; } = new();

        public PlcHostOptions MapTag(string name, string address, PlcDataType type, int stringLength = 0)
        {
            Tags.Add(new TagDefinition(name, address, type, stringLength));
            return this;
        }

        public PlcHostOptions MapRegion(string regionKey, string area, int startOffset, int length)
        {
            Regions.Add(new RegionConfig(regionKey, area, startOffset, length));
            return this;
        }
    }
}
