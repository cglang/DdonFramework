using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.VitrinPLC.Clients.Mitsubishi;

public abstract class McProtocolBase : IMcProtocol
{
    private McProtocolCommand ProtocolCommand { get; set; }
    
    public abstract bool Connected { get; }
    public McProtocolFrame CommandProtocolFrame { get; init; }
    public string HostName { get; set; }
    public int Port { get; set; }

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
    
    public async Task<int> WriteDeviceBlock(McProtocolDeviceType type, int address, int size, byte[] data)
    {
        var (sdCommand, length) = BuildWriteFrame(address, type, size, data);
        var rtResponse = await TryExecution(sdCommand, length);
        var rtCode = ProtocolCommand.SetResponse(rtResponse);
        return rtCode;
    }
    
    public async Task<byte[]> ReadDeviceBlock(McProtocolDeviceType type, int address, int size)
    {
        var (command, minLength) = BuildReadFrame(address, type, size);
        var response = await TryExecution(command, minLength);
        ProtocolCommand.SetResponse(response);
        return ProtocolCommand.Response;
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

    protected abstract Task DoConnectAsync(CancellationToken cancellationToken = default);
    protected abstract void DoDisconnect();
    protected abstract Task<byte[]> ExecuteAsync(byte[] iCommand);

    private (byte[] command, int minLength) BuildReadFrame(int address, McProtocolDeviceType type, int points)
    {
        var data = new[] { (byte)address, (byte)(address >> 8), (byte)(address >> 16), (byte)type, (byte)points, (byte)(points >> 8) };
        return CommandProtocolFrame switch
        {
            McProtocolFrame.Mc3E => (ProtocolCommand.SetCommandMc3E(0x0401, 0x0000, data), 11),
            McProtocolFrame.Mc4E => (ProtocolCommand.SetCommandMc4E(0x0401, 0x0000, data), 15),
            McProtocolFrame.Mc1E => (ProtocolCommand.SetCommandMc1E(0x01, BuildAddressData1E(address, points)), 2),
            _ => throw new Exception("Message frame not supported")
        };
    }

    private (byte[] command, int minLength) BuildWriteFrame(int address, McProtocolDeviceType type, int points, byte[] payload)
    {
        var data = new byte[6 + payload.Length];
        data[0] = (byte)address; data[1] = (byte)(address >> 8); data[2] = (byte)(address >> 16);
        data[3] = (byte)type; data[4] = (byte)points; data[5] = (byte)(points >> 8);
        Buffer.BlockCopy(payload, 0, data, 6, payload.Length);

        return CommandProtocolFrame switch
        {
            McProtocolFrame.Mc3E => (ProtocolCommand.SetCommandMc3E(0x1401, 0x0000, data), 11),
            McProtocolFrame.Mc4E => (ProtocolCommand.SetCommandMc4E(0x1401, 0x0000, data), 15),
            McProtocolFrame.Mc1E => (ProtocolCommand.SetCommandMc1E(0x03, BuildWriteData1E(address, points, payload)), 2),
            _ => throw new Exception("Message frame not supported")
        };
    }

    private static byte[] BuildAddressData1E(int address, int points)
    {
        return new byte[] { (byte)address, (byte)(address >> 8), (byte)(address >> 16), (byte)(address >> 24), 0x20, 0x44, (byte)points, 0x00 };
    }

    private static byte[] BuildWriteData1E(int address, int points, byte[] payload)
    {
        var data = new byte[8 + payload.Length];
        data[0] = (byte)address; data[1] = (byte)(address >> 8); data[2] = (byte)(address >> 16); data[3] = (byte)(address >> 24);
        data[4] = 0x20; data[5] = 0x44; data[6] = (byte)points; data[7] = 0x00;
        Buffer.BlockCopy(payload, 0, data, 8, payload.Length);
        return data;
    }

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
                throw new Exception("无法从PLC获取正确的数值。");
            }
        } while (ProtocolCommand.IsIncorrectResponse(rtResponse, minlength));

        return rtResponse;
    }
}