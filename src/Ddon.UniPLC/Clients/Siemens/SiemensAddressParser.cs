using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Models;
using System.Text.RegularExpressions;

namespace Ddon.UniPLC.Clients.Siemens;

/// <summary>
/// Siemens 地址解析器
/// </summary>
public class SiemensAddressParser
{
    /// <summary>
    /// 解析 Siemens 地址格式
    /// 支持格式：
    /// - DB1.DBX0.0 (DB块位寻址)
    /// - DB1.DBW0 (DB块字寻址)
    /// - DB1.DBD0 (DB块双字寻址)
    /// - M0.0 (M区位寻址)
    /// - M0 (M区字寻址)
    /// - I0.0 (I区位寻址)
    /// - Q0.0 (Q区位寻址)
    /// </summary>
    public static PlcAddress Parse(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentNullException(nameof(address));

        var plcAddress = new PlcAddress { RawAddress = address };

        // DB 块寻址
        if (address.StartsWith("DB", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(address, @"DB(\d+)\.DBX(\d+)\.(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                plcAddress.Area = "DB";
                plcAddress.BlockNumber = int.Parse(match.Groups[1].Value);
                plcAddress.Offset = int.Parse(match.Groups[2].Value);
                plcAddress.Bit = int.Parse(match.Groups[3].Value);
                plcAddress.DataType = PlcDataType.Bool;
                return plcAddress;
            }

            match = Regex.Match(address, @"DB(\d+)\.DBW(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                plcAddress.Area = "DB";
                plcAddress.BlockNumber = int.Parse(match.Groups[1].Value);
                plcAddress.Offset = int.Parse(match.Groups[2].Value);
                plcAddress.DataType = PlcDataType.UShort;
                return plcAddress;
            }

            match = Regex.Match(address, @"DB(\d+)\.DBD(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                plcAddress.Area = "DB";
                plcAddress.BlockNumber = int.Parse(match.Groups[1].Value);
                plcAddress.Offset = int.Parse(match.Groups[2].Value);
                plcAddress.DataType = PlcDataType.Int;
                return plcAddress;
            }
        }

        // M 区寻址
        if (address.StartsWith("M", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(address, @"M(\d+)\.(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                plcAddress.Area = "M";
                plcAddress.Offset = int.Parse(match.Groups[1].Value);
                plcAddress.Bit = int.Parse(match.Groups[2].Value);
                plcAddress.DataType = PlcDataType.Bool;
                return plcAddress;
            }

            var match2 = Regex.Match(address, @"M(\d+)", RegexOptions.IgnoreCase);
            if (match2.Success)
            {
                plcAddress.Area = "M";
                plcAddress.Offset = int.Parse(match2.Groups[1].Value);
                plcAddress.DataType = PlcDataType.Byte;
                return plcAddress;
            }
        }

        // I 区寻址
        if (address.StartsWith("I", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(address, @"I(\d+)\.(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                plcAddress.Area = "I";
                plcAddress.Offset = int.Parse(match.Groups[1].Value);
                plcAddress.Bit = int.Parse(match.Groups[2].Value);
                plcAddress.DataType = PlcDataType.Bool;
                return plcAddress;
            }
        }

        // Q 区寻址
        if (address.StartsWith("Q", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(address, @"Q(\d+)\.(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                plcAddress.Area = "Q";
                plcAddress.Offset = int.Parse(match.Groups[1].Value);
                plcAddress.Bit = int.Parse(match.Groups[2].Value);
                plcAddress.DataType = PlcDataType.Bool;
                return plcAddress;
            }
        }

        throw new ArgumentException($"Invalid Siemens address format: {address}");
    }
}
