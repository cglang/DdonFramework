namespace Ddon.Serial.Abstractions
{
    public interface ISerialProtocol
    {
        byte[] Encode(object message);

        (byte[]? Frame, int Consumed) Decode(byte[] buffer, int offset, int count);
    }
}
