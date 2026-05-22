using System;
using System.Collections.Generic;
using Ddon.VitrinPLC;
using Ddon.VitrinPLC.Clients;
using Ddon.VitrinPLC.Models;

namespace Plc.Hosting
{
    // ─────────────────────────────────────────────
    // 配置构建器 DSL
    // ─────────────────────────────────────────────
    public sealed class PlcMirrorOptions
    {
        internal PlcClientType Protocol { get; set; } = PlcClientType.None;
        internal SiemensOptions Siemens { get; set; }
        internal MitsubishiOptions Mitsubishi { get; set; }
        internal OmronOptions Omron { get; set; }

        public int ScanInterval { get; set; } = 200;

        internal List<TagDefinition> Tags { get; } = new();
        internal List<RegionConfig> Regions { get; } = new();

        public PlcMirrorOptions UseSiemens(string name, Action<SiemensOptions> configure)
        {
            Protocol = PlcClientType.Siemens;
            Siemens = new SiemensOptions { Name = name };
            configure(Siemens);
            return this;
        }

        public PlcMirrorOptions UseMitsubishi(string name, Action<MitsubishiOptions> configure)
        {
            Protocol = PlcClientType.Mitsubishi;
            Mitsubishi = new MitsubishiOptions { Name = name };
            configure(Mitsubishi);
            return this;
        }

        public PlcMirrorOptions UseOmron(string name, Action<OmronOptions> configure)
        {
            Protocol = PlcClientType.Omron;
            Omron = new OmronOptions { Name = name };
            configure(Omron);
            return this;
        }

        public PlcMirrorOptions MapTag(string name, string address, PlcDataType type, int stringLength = 0)
        {
            Tags.Add(new TagDefinition(name, address, type, stringLength));
            return this;
        }

        public PlcMirrorOptions MapRegion(string regionKey, string area, int startOffset, int length)
        {
            Regions.Add(new RegionConfig(regionKey, area, startOffset, length));
            return this;
        }
    }
}
