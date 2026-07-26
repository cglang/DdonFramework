using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.VitrinPLC.Clients.Mitsubishi;

public abstract class McProtocolBase : IMcProtocol
{
    public abstract bool Connected { get; }
    public McProtocolFrame CommandProtocolFrame { get; init; }
    public string HostName { get; set; }
    public int Port { get; set; }
    public int Device { private set; get; }

    protected McProtocolBase(string hostName, int port, McProtocolFrame protocolFrame)
    {
        CommandProtocolFrame = protocolFrame;
        HostName = hostName;
        Port = port;
        ProtocolCommand = new McProtocolCommand(CommandProtocolFrame);
    }


    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        await DoConnectAsync(cancellationToken);
    }

    public int Close()
    {
        DoDisconnect();
        return 0;
    }

    public void Dispose()
    {
        Close();
    }

    public async Task<int> SetBitDevice(string iDeviceName, int iSize, int[] iData)
    {
        GetDeviceCode(iDeviceName, out var type, out var addr);
        return await SetBitDevice(type, addr, iSize, iData);
    }

    public async Task<int> SetBitDevice(McProtocolDeviceType type, int iAddress, int iSize, int[] iData)
    {
        var data = new List<byte>(6)
        {
            (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)type, (byte)iSize, (byte)(iSize >> 8)
        };
        var d = (byte)iData[0];
        var i = 0;
        while (i < iData.Length)
        {
            if (i % 2 == 0)
            {
                d = (byte)iData[i];
                d <<= 4;
            }
            else
            {
                d |= (byte)(iData[i] & 0x01);
                data.Add(d);
            }

            ++i;
        }

        if (i % 2 != 0)
        {
            data.Add(d);
        }

        var length = (int)ProtocolCommand.ProtocolFrameType;
        var sdCommand = ProtocolCommand.SetCommandMc3E(0x1401, 0x0001, data.ToArray());
        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        return rtCode;
    }

    public async Task<int> GetBitDevice(string iDeviceName, int iSize, int[] oData)
    {
        GetDeviceCode(iDeviceName, out var type, out var addr);
        return await GetBitDevice(type, addr, iSize, oData);
    }

    public async Task<int> GetBitDevice(McProtocolDeviceType type, int address, int size, int[] data)
    {
        var bytes = new List<byte>(6)
        {
            (byte)address, (byte)(address >> 8), (byte)(address >> 16), (byte)type, (byte)size, (byte)(size >> 8)
        };
        var sdCommand = ProtocolCommand.SetCommandMc3E(0x0401, 0x0001, bytes.ToArray());
        var length = (ProtocolCommand.ProtocolFrameType == McProtocolFrame.Mc3E) ? 11 : 15;
        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        var rtData = ProtocolCommand.Response;
        for (var i = 0; i < size; ++i)
        {
            if (i % 2 == 0)
            {
                data[i] = (rtCode == 0) ? ((rtData[i / 2] >> 4) & 0x01) : 0;
            }
            else
            {
                data[i] = (rtCode == 0) ? (rtData[i / 2] & 0x01) : 0;
            }
        }

        return rtCode;
    }

    public async Task<int> WriteDeviceBlock(string iDeviceName, int iSize, int[] iData)
    {
        GetDeviceCode(iDeviceName, out var type, out var addr);
        return await WriteDeviceBlock(type, addr, iSize, iData);
    }

    public async Task<int> WriteDeviceBlock(McProtocolDeviceType iType, int iAddress, int iSize, int[] iData)
    {
        List<byte> data;

        var deviceData = new List<byte>();
        foreach (int t in iData)
        {
            deviceData.Add((byte)t);
            deviceData.Add((byte)(t >> 8));
        }

        byte[] sdCommand;
        int length;
        switch (CommandProtocolFrame)
        {
            case McProtocolFrame.Mc3E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)iSize,
                    (byte)(iSize >> 8)
                };
                data.AddRange(deviceData.ToArray());
                sdCommand = ProtocolCommand.SetCommandMc3E(0x1401, 0x0000, data.ToArray());
                length = 11;
                break;
            case McProtocolFrame.Mc4E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)iSize,
                    (byte)(iSize >> 8)
                };
                data.AddRange(deviceData.ToArray());
                sdCommand = ProtocolCommand.SetCommandMc4E(0x1401, 0x0000, data.ToArray());
                length = 15;
                break;
            case McProtocolFrame.Mc1E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)(iAddress >> 24), 0x20, 0x44,
                    (byte)iSize,
                    0x00
                };
                data.AddRange(deviceData.ToArray());
                sdCommand = ProtocolCommand.SetCommandMc1E(0x03, data.ToArray());
                length = 2;
                break;
            default:
                throw new Exception("Message frame not supported");
        }

        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        return rtCode;
    }

    public async Task<int> WriteDeviceBlock(McProtocolDeviceType iType, int iAddress, int devicePoints, byte[] bData)
    {
        List<byte> data;
        byte[] sdCommand;
        int length;
        switch (CommandProtocolFrame)
        {
            case McProtocolFrame.Mc3E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)devicePoints,
                    (byte)(devicePoints >> 8)
                };
                data.AddRange(bData);
                sdCommand = ProtocolCommand.SetCommandMc3E(0x1401, 0x0000, data.ToArray());
                length = 11;
                break;
            case McProtocolFrame.Mc4E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)devicePoints,
                    (byte)(devicePoints >> 8)
                };
                data.AddRange(bData);
                sdCommand = ProtocolCommand.SetCommandMc4E(0x1401, 0x0000, data.ToArray());
                length = 15;
                break;
            case McProtocolFrame.Mc1E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)(iAddress >> 24), 0x20, 0x44,
                    (byte)devicePoints,
                    0x00
                };
                data.AddRange(bData);
                sdCommand = ProtocolCommand.SetCommandMc1E(0x03, data.ToArray());
                length = 2;
                break;
            default:
                throw new Exception("Message frame not supported");
        }

        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        return rtCode;
    }

    public async Task<byte[]> ReadDeviceBlock(string iDeviceName, int iSize, int[] oData)
    {
        GetDeviceCode(iDeviceName, out var type, out var addr);
        return await ReadDeviceBlock(type, addr, iSize, oData);
    }

    public async Task<byte[]> ReadDeviceBlock(McProtocolDeviceType iType, int iAddress, int iSize, int[] oData)
    {
        List<byte> data;
        byte[] sdCommand;
        int length;

        switch (CommandProtocolFrame)
        {
            case McProtocolFrame.Mc3E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)iSize,
                    (byte)(iSize >> 8)
                };
                sdCommand = ProtocolCommand.SetCommandMc3E(0x0401, 0x0000, data.ToArray());
                length = 11;
                break;
            case McProtocolFrame.Mc4E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)iSize,
                    (byte)(iSize >> 8)
                };
                sdCommand = ProtocolCommand.SetCommandMc4E(0x0401, 0x0000, data.ToArray());
                length = 15;
                break;
            case McProtocolFrame.Mc1E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)(iAddress >> 24), 0x20, 0x44,
                    (byte)iSize,
                    0x00
                };
                sdCommand = ProtocolCommand.SetCommandMc1E(0x01, data.ToArray());
                length = 2;
                break;
            default:
                throw new Exception("Message frame not supported");
        }

        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        var rtData = ProtocolCommand.Response;
        for (var i = 0; i < iSize; ++i)
        {
            oData[i] = (rtCode == 0) ? BitConverter.ToInt16(rtData, i * 2) : 0;
        }

        return rtData;
    }

    public async Task<byte[]> ReadDeviceBlock(McProtocolDeviceType iType, int iAddress, int devicePoints)
    {
        List<byte> data;
        byte[] sdCommand;
        int length;

        switch (CommandProtocolFrame)
        {
            case McProtocolFrame.Mc3E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)devicePoints,
                    (byte)(devicePoints >> 8)
                };
                sdCommand = ProtocolCommand.SetCommandMc3E(0x0401, 0x0000, data.ToArray());
                length = 11;
                break;
            case McProtocolFrame.Mc4E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, (byte)devicePoints,
                    (byte)(devicePoints >> 8)
                };
                sdCommand = ProtocolCommand.SetCommandMc4E(0x0401, 0x0000, data.ToArray());
                length = 15;
                break;
            case McProtocolFrame.Mc1E:
                data = new List<byte>(6)
                {
                    (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)(iAddress >> 24), 0x20, 0x44,
                    (byte)devicePoints,
                    0x00
                };
                sdCommand = ProtocolCommand.SetCommandMc1E(0x01, data.ToArray());
                length = 2;
                break;
            default:
                throw new Exception("Message frame not supported");
        }

        var rtResponse = await TryExecution(sdCommand, length);
        ProtocolCommand.SetResponse(rtResponse);
        var rtData = ProtocolCommand.Response;
        return rtData;
    }

    public async Task<int> SetDevice(string iDeviceName, int iData)
    {
        GetDeviceCode(iDeviceName, out var type, out var addr);
        return await SetDevice(type, addr, iData);
    }

    public async Task<int> SetDevice(McProtocolDeviceType iType, int iAddress, int iData)
    {
        var data = new List<byte>(6)
        {
            (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, 0x01, 0x00, (byte)iData,
            (byte)(iData >> 8)
        };
        var sdCommand = ProtocolCommand.SetCommandMc3E(0x1401, 0x0000, data.ToArray());
        var length = (ProtocolCommand.ProtocolFrameType == McProtocolFrame.Mc3E) ? 11 : 15;
        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        return rtCode;
    }

    public async Task<int> GetDevice(string iDeviceName)
    {
        GetDeviceCode(iDeviceName, out var type, out var addr);
        return await GetDevice(type, addr);
    }

    public async Task<int> GetDevice(McProtocolDeviceType iType, int iAddress)
    {
        var data = new List<byte>(6)
        {
            (byte)iAddress, (byte)(iAddress >> 8), (byte)(iAddress >> 16), (byte)iType, 0x01, 0x00
        };
        var sdCommand = ProtocolCommand.SetCommandMc3E(0x0401, 0x0000, data.ToArray());
        var length = (ProtocolCommand.ProtocolFrameType == McProtocolFrame.Mc3E) ? 11 : 15;
        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        if (0 < rtCode)
        {
            this.Device = 0;
        }
        else
        {
            var rtData = ProtocolCommand.Response;
            this.Device = BitConverter.ToInt16(rtData, 0);
        }

        return rtCode;
    }

    public static McProtocolDeviceType GetDeviceType(string s)
    {
        return s switch
        {
            "M" => McProtocolDeviceType.M,
            "SM" => McProtocolDeviceType.SM,
            "L" => McProtocolDeviceType.L,
            "F" => McProtocolDeviceType.F,
            "V" => McProtocolDeviceType.V,
            "S" => McProtocolDeviceType.S,
            "X" => McProtocolDeviceType.X,
            "Y" => McProtocolDeviceType.Y,
            "B" => McProtocolDeviceType.B,
            "SB" => McProtocolDeviceType.SB,
            "DX" => McProtocolDeviceType.DX,
            "DY" => McProtocolDeviceType.DY,
            "D" => McProtocolDeviceType.D,
            "SD" => McProtocolDeviceType.SD,
            "R" => McProtocolDeviceType.R,
            "ZR" => McProtocolDeviceType.ZR,
            "W" => McProtocolDeviceType.W,
            "SW" => McProtocolDeviceType.SW,
            "TC" => McProtocolDeviceType.TC,
            "TS" => McProtocolDeviceType.TS,
            "TN" => McProtocolDeviceType.TN,
            "CC" => McProtocolDeviceType.CC,
            "CS" => McProtocolDeviceType.CS,
            "CN" => McProtocolDeviceType.CN,
            "SC" => McProtocolDeviceType.SC,
            "SS" => McProtocolDeviceType.SS,
            "SN" => McProtocolDeviceType.SN,
            "Z" => McProtocolDeviceType.Z,
            "TT" => McProtocolDeviceType.TT,
            "TM" => McProtocolDeviceType.TM,
            "CT" => McProtocolDeviceType.CT,
            "CM" => McProtocolDeviceType.CM,
            "A" => McProtocolDeviceType.A,
            _ => McProtocolDeviceType.Max
        };
    }

    public static bool IsBitDevice(McProtocolDeviceType type)
    {
        return type is not (McProtocolDeviceType.D or McProtocolDeviceType.SD or McProtocolDeviceType.Z
            or McProtocolDeviceType.ZR or McProtocolDeviceType.R or McProtocolDeviceType.W);
    }

    public static bool IsHexDevice(McProtocolDeviceType type)
    {
        return type is McProtocolDeviceType.X or McProtocolDeviceType.Y or McProtocolDeviceType.B
            or McProtocolDeviceType.W;
    }

    public static void GetDeviceCode(string iDeviceName, out McProtocolDeviceType oType, out int oAddress)
    {
        var s = iDeviceName.ToUpper();
        string strAddress;

        var strType = s.Substring(0, 1);
        switch (strType)
        {
            case "A":
            case "B":
            case "D":
            case "F":
            case "L":
            case "M":
            case "R":
            case "V":
            case "W":
            case "X":
            case "Y":
                strAddress = s.Substring(1);
                break;
            case "Z":
                strType = s.Substring(0, 2);
                strAddress = s.Substring(strType.Equals("ZR") ? 2 : 1);
                break;
            case "C":
                strType = s.Substring(0, 2);
                switch (strType)
                {
                    case "CC":
                    case "CM":
                    case "CN":
                    case "CS":
                    case "CT":
                        strAddress = s.Substring(2);
                        break;
                    default:
                        throw new Exception("Invalid format.");
                }

                break;
            case "S":
                strType = s.Substring(0, 2);
                switch (strType)
                {
                    case "SD":
                    case "SM":
                        strAddress = s.Substring(2);
                        break;
                    default:
                        throw new Exception("Invalid format.");
                }

                break;
            case "T":
                strType = s.Substring(0, 2);
                switch (strType)
                {
                    case "TC":
                    case "TM":
                    case "TN":
                    case "TS":
                    case "TT":
                        strAddress = s.Substring(2);
                        break;
                    default:
                        throw new Exception("Invalid format.");
                }

                break;
            default:
                throw new Exception("Invalid format.");
        }

        oType = GetDeviceType(strType);
        oAddress = IsHexDevice(oType) ? Convert.ToInt32(strAddress, BlockSize) : Convert.ToInt32(strAddress);
    }

    protected abstract Task DoConnectAsync(CancellationToken cancellationToken = default);
    protected abstract void DoDisconnect();
    protected abstract Task<byte[]> ExecuteAsync(byte[] iCommand);

    private const int BlockSize = 0x0010;
    private McProtocolCommand ProtocolCommand { get; set; }

    private async Task<byte[]> TryExecution(byte[] iCommand, int minlength)
    {
        byte[] rtResponse;
        var tCount = 10;
        do
        {
            rtResponse = await ExecuteAsync(iCommand);
            --tCount;
            if (tCount < 0)
            {
                throw new Exception("PLCから正しい値が取得できません.");
            }
        } while (ProtocolCommand.IsIncorrectResponse(rtResponse, minlength));

        return rtResponse;
    }
}