namespace Ddon.Socket.Abstractions
{
    public interface ISocketProtocol
    {
        byte[] Encode(object message);

        (byte[]? Frame, int Consumed) Decode(byte[] buffer, int offset, int count);
    }
}
