using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.AddressParsers;

public sealed class OmronAddressParser : IPlcAddressParser
{
    private static readonly Regex AddressRegex = new(
        @"^(?<area>[A-Za-z]+)(?<offset>\d+)$",
        RegexOptions.Compiled);

    private static readonly HashSet<string> ValidAreas = new(StringComparer.OrdinalIgnoreCase)
        { "D", "W", "H", "CIO", "DM", "C", "T" };

    public ParsedAddress Parse(string address, PlcDataType type)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        var m = AddressRegex.Match(address.Trim());
        if (!m.Success)
            throw new NotSupportedException($"不支持的欧姆龙地址格式: '{address}'");

        string area = m.Groups["area"].Value.ToUpper();
        int offset = int.Parse(m.Groups["offset"].Value);

        if (!ValidAreas.Contains(area))
            throw new NotSupportedException($"不支持的欧姆龙区域: '{area}'");

        return new ParsedAddress
        {
            Original = address,
            RegionKey = area,
            Area = area,
            ByteOffset = offset * 2,
            BitIndex = 0,
            DataType = type,
            IsBit = false
        };
    }
}
