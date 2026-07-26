using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    public interface IPlcAddressParser
    {
        ParsedAddress Parse(string address, PlcDataType type);
    }
}


