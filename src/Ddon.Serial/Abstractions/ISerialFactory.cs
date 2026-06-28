using System;
using Ddon.Serial.Configuration;

namespace Ddon.Serial.Abstractions
{
    public interface ISerialFactory
    {
        ISerialWorker CreateWorker(SerialPortOptions options);

        ISerialProtocol CreateProtocol(Type protocolType);

        IReconnectStrategy CreateReconnectStrategy(Type strategyType);
    }
}
