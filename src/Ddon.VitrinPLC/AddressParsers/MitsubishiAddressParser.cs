using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.AddressParsers;

public sealed class MitsubishiAddressParser : IPlcAddressParser
{
    private static readonly Regex AddressRegex = new(
        @"^(?<area>[A-Za-z]+)(?<offset>\d+)$",
        RegexOptions.Compiled);

    private static readonly HashSet<string> WordAreas = new(StringComparer.OrdinalIgnoreCase)
        { "D", "W", "R", "SD" };

    private static readonly HashSet<string> BitAreas = new(StringComparer.OrdinalIgnoreCase)
        { "M", "X", "Y", "B", "SM" };

    public ParsedAddress Parse(string address, PlcDataType type)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        var m = AddressRegex.Match(address.Trim());
        if (!m.Success)
            throw new NotSupportedException($"不支持的三菱地址格式: '{address}'");

        string area = m.Groups["area"].Value.ToUpper();
        int offset = int.Parse(m.Groups["offset"].Value);

        if (!WordAreas.Contains(area) && !BitAreas.Contains(area))
            throw new NotSupportedException($"不支持的三菱软元件区域: '{area}'");

        var isBit = BitAreas.Contains(area);
        return new ParsedAddress
        {
            Original = address,
            RegionKey = area,
            Area = area,
            ByteOffset = isBit ? offset / 8 : offset * 2,
            BitIndex = isBit ? offset % 8 : 0,
            DataType = type,
            IsBit = isBit
        };
    }
}
