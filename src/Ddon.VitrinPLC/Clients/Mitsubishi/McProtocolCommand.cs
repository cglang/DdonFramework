using System;
using System.Collections.Generic;

namespace Ddon.VitrinPLC.Clients.Mitsubishi;

public class McProtocolCommand
{
    public McProtocolFrame ProtocolFrameType { get; private set; }
    private uint SerialNumber { get; set; }
    private uint NetworkNumber { get; set; }
    private uint PcNumber { get; set; }
    private uint IoNumber { get; set; }
    private uint ChannelNumber { get; set; }
    private uint CpuTimer { get; set; }
    private int ResultCode { get; set; }
    public byte[] Response { get; private set; } = null!;

    public McProtocolCommand(McProtocolFrame iProtocolFrame)
    {
        ProtocolFrameType = iProtocolFrame;
        SerialNumber = 0x0001u;
        NetworkNumber = 0x0000u;
        PcNumber = 0x00FFu;
        IoNumber = 0x03FFu;
        ChannelNumber = 0x0000u;
        CpuTimer = 0x0010u;
    }

    public byte[] SetCommandMc1E(byte subheader, byte[] data)
    {
        var ret = new List<byte>(data.Length + 4);
        ret.Add(subheader);
        ret.Add((byte)this.PcNumber);
        ret.Add((byte)CpuTimer);
        ret.Add((byte)(CpuTimer >> 8));
        ret.AddRange(data);
        return ret.ToArray();
    }

    public byte[] SetCommandMc3E(uint mainCommand, uint subCommand, byte[] data)
    {
        var dataLength = (uint)(data.Length + 6);
        var ret = new List<byte>(data.Length + 20);
        const uint frame = 0x0050u;
        ret.Add((byte)frame);
        ret.Add(0x00);

        ret.Add((byte)NetworkNumber);

        ret.Add((byte)PcNumber);

        ret.Add((byte)IoNumber);
        ret.Add((byte)(IoNumber >> 8));
        ret.Add((byte)ChannelNumber);
        ret.Add((byte)dataLength);
        ret.Add((byte)(dataLength >> 8));

        ret.Add((byte)CpuTimer);
        ret.Add((byte)(CpuTimer >> 8));
        ret.Add((byte)mainCommand);
        ret.Add((byte)(mainCommand >> 8));
        ret.Add((byte)subCommand);
        ret.Add((byte)(subCommand >> 8));

        ret.AddRange(data);
        return ret.ToArray();
    }

    public byte[] SetCommandMc4E(uint iMainCommand, uint iSubCommand, byte[] iData)
    {
        var dataLength = (uint)(iData.Length + 6);
        var ret = new List<byte>(iData.Length + 20);
        const uint frame = 0x0054u;
        ret.Add((byte)frame);
        ret.Add(0x00);
        ret.Add((byte)SerialNumber);
        ret.Add((byte)(SerialNumber >> 8));
        ret.Add(0x00);
        ret.Add(0x00);
        ret.Add((byte)NetworkNumber);
        ret.Add((byte)PcNumber);
        ret.Add((byte)IoNumber);
        ret.Add((byte)(IoNumber >> 8));
        ret.Add((byte)ChannelNumber);
        ret.Add((byte)dataLength);
        ret.Add((byte)(dataLength >> 8));
        ret.Add((byte)CpuTimer);
        ret.Add((byte)(CpuTimer >> 8));
        ret.Add((byte)iMainCommand);
        ret.Add((byte)(iMainCommand >> 8));
        ret.Add((byte)iSubCommand);
        ret.Add((byte)(iSubCommand >> 8));

        ret.AddRange(iData);
        return ret.ToArray();
    }

    public int SetResponse(byte[] iResponse)
    {
        int min;
        switch (ProtocolFrameType)
        {
            case McProtocolFrame.Mc1E:
                min = 2;
                if (min <= iResponse.Length)
                {
                    ResultCode = iResponse[min - 2];
                    Response = new byte[iResponse.Length - 2];
                    Buffer.BlockCopy(iResponse, min, Response, 0, Response.Length);
                }

                break;
            case McProtocolFrame.Mc3E:
                min = 11;
                if (min <= iResponse.Length)
                {
                    var btCount = new[] { iResponse[min - 4], iResponse[min - 3] };
                    var btCode = new[] { iResponse[min - 2], iResponse[min - 1] };
                    int rsCount = BitConverter.ToUInt16(btCount, 0);
                    ResultCode = BitConverter.ToUInt16(btCode, 0);
                    Response = new byte[rsCount - 2];
                    Buffer.BlockCopy(iResponse, min, Response, 0, Response.Length);
                }

                break;
            case McProtocolFrame.Mc4E:
                min = 15;
                if (min <= iResponse.Length)
                {
                    var btCount = new[] { iResponse[min - 4], iResponse[min - 3] };
                    var btCode = new[] { iResponse[min - 2], iResponse[min - 1] };
                    int rsCount = BitConverter.ToUInt16(btCount, 0);
                    ResultCode = BitConverter.ToUInt16(btCode, 0);
                    Response = new byte[rsCount - 2];
                    Buffer.BlockCopy(iResponse, min, Response, 0, Response.Length);
                }

                break;
            default:
                throw new Exception("Frame type not supported.");
        }

        return ResultCode;
    }

    public bool IsIncorrectResponse(byte[] response, int minLenght)
    {
        switch (this.ProtocolFrameType)
        {
            case McProtocolFrame.Mc1E:
                return response.Length < minLenght;

            case McProtocolFrame.Mc3E:
            case McProtocolFrame.Mc4E:
                var btCount = new[] { response[minLenght - 4], response[minLenght - 3] };
                var btCode = new[] { response[minLenght - 2], response[minLenght - 1] };
                var rsCount = BitConverter.ToUInt16(btCount, 0) - 2;
                var rsCode = BitConverter.ToUInt16(btCode, 0);
                return (rsCode == 0 && rsCount != (response.Length - minLenght));

            default:
                throw new Exception("Type Not supported");
        }
    }
}