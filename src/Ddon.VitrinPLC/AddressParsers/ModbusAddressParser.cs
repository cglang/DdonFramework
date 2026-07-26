using System;
using System.Text.RegularExpressions;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.AddressParsers;

public sealed class ModbusAddressParser : IPlcAddressParser
{
    private static readonly Regex ModbusRegex = new(@"^\d{6}$", RegexOptions.Compiled);

    public ParsedAddress Parse(string address, PlcDataType type)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));

        if (!ModbusRegex.IsMatch(address.Trim()))
            throw new NotSupportedException($"不支持的 Modbus 地址格式: '{address}'");

        int raw = int.Parse(address.Trim());
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
}
