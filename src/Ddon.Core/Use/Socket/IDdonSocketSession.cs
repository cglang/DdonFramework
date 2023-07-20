using System;
using System.Threading.Tasks;

namespace Ddon.Core.Use.Socket
{
    public interface IDdonSocketSession
    {
        Guid SocketId { get; }

        void Start();

        Task StartAsync();

        ValueTask SendBytesAsync(byte[] data, DataType type = DataType.Byte);

        ValueTask SendStringAsync(string data);
    }
}
