using System;
using System.Text.RegularExpressions;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.AddressParsers;

public sealed class SiemensAddressParser : IPlcAddressParser
{
    private static readonly Regex SiemensDbRegex = new(
        @"^DB(?<db>\d+)\.DB(?<dtype>[DWBX])(?<offset>\d+)(?:\.(?<bit>\d+))?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SiemensMRegex = new(
        @"^M(?<width>[WD]?)(?<offset>\d+)(?:\.(?<bit>\d+))?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ParsedAddress Parse(string address, PlcDataType type)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        address = address.Trim();

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

        throw new NotSupportedException($"不支持的西门子地址格式: '{address}'");
    }
}
