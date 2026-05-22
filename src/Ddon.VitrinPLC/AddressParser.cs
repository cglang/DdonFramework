using System;
using System.Text.RegularExpressions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC
{
    /// <summary>
    /// 统一地址解析器。
    /// 支持：
    ///   Siemens   DB1.DBD0 / DB1.DBX10.5 / DB1.DBW4 / DB1.DBB2 / M0.0 / MW10 / MD20
    ///   Mitsubishi D100 / M200 / X0 / Y0
    ///   Omron     D100 / W0 / CIO0
    ///   Modbus    400001 / 000001
    /// </summary>
    public static class AddressParser
    {
        // Siemens DB: DB1.DBD0  DB1.DBX10.5  DB1.DBW4  DB1.DBB2
        private static readonly Regex SiemensDbRegex =
            new(@"^DB(?<db>\d+)\.DB(?<dtype>[DWBX])(?<offset>\d+)(?:\.(?<bit>\d+))?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Siemens M区: M0.0  MW10  MD20
        private static readonly Regex SiemensMRegex =
            new(@"^M(?<width>[WD]?)(?<offset>\d+)(?:\.(?<bit>\d+))?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static ParsedAddress Parse(string address, PlcDataType type)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));

            address = address.Trim();

            // ── Siemens DB 区 ─────────────────────────────
            var m = SiemensDbRegex.Match(address);
            if (m.Success)
            {
                int db = int.Parse(m.Groups["db"].Value);
                string dtyp = m.Groups["dtype"].Value.ToUpper();
                int offset = int.Parse(m.Groups["offset"].Value);
                int bit = m.Groups["bit"].Success ? int.Parse(m.Groups["bit"].Value) : 0;

                return new ParsedAddress
                {
                    Original = address,
                    RegionKey = $"DB{db}",
                    Area = $"DB{db}",
                    ByteOffset = offset,
                    BitIndex = bit,
                    DataType = type,
                    IsBit = dtyp == "X"
                };
            }

            // ── Siemens M 区 ──────────────────────────────
            m = SiemensMRegex.Match(address);
            if (m.Success)
            {
                string width = m.Groups["width"].Value.ToUpper();
                int offset = int.Parse(m.Groups["offset"].Value);
                int bit = m.Groups["bit"].Success ? int.Parse(m.Groups["bit"].Value) : 0;

                return new ParsedAddress
                {
                    Original = address,
                    RegionKey = "M",
                    Area = "M",
                    ByteOffset = width == "W" ? offset * 2 : width == "D" ? offset * 4 : offset,
                    BitIndex = bit,
                    DataType = type,
                    IsBit = bit >= 0 && width == ""
                };
            }

            // ── Mitsubishi / Omron D 区 ───────────────────
            if (Regex.IsMatch(address, @"^[DWC]?\w+\d+$", RegexOptions.IgnoreCase))
            {
                var rMatch = Regex.Match(address, @"^(?<area>[A-Za-z]+)(?<offset>\d+)$");
                if (rMatch.Success)
                {
                    string area = rMatch.Groups["area"].Value.ToUpper();
                    int offset = int.Parse(rMatch.Groups["offset"].Value);
                    return new ParsedAddress
                    {
                        Original = address,
                        RegionKey = area,
                        Area = area,
                        ByteOffset = offset * 2,          // 三菱 D 寄存器 = Word = 2 bytes
                        BitIndex = 0,
                        DataType = type,
                        IsBit = false
                    };
                }
            }

            // ── Modbus: 4xxxxx (保持寄存器) / 0xxxxx (线圈) ─
            if (Regex.IsMatch(address, @"^\d{6}$"))
            {
                int raw = int.Parse(address);
                int offset = (raw % 100000 - 1) * 2;
                bool isBit = raw < 100000;
                return new ParsedAddress
                {
                    Original = address,
                    RegionKey = isBit ? "COIL" : "HOLDING",
                    Area = isBit ? "COIL" : "HOLDING",
                    ByteOffset = offset,
                    BitIndex = 0,
                    DataType = type,
                    IsBit = isBit
                };
            }

            throw new NotSupportedException($"无法解析地址格式: '{address}'");
        }

        public static int GetByteSize(PlcDataType type, int stringLength = 0) => type switch
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
}
