namespace Ddon.OpenProtocol.Configuration
{
    public enum MessageTerminator
    {
        None,
        Nul,
        CrLf,
        Custom,
    }

    public class OpenProtocolEndpointOptions
    {
        public string Name { get; set; } = "default";

        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 4545;

        public int ConnectTimeoutMs { get; set; } = 5000;

        public int RequestTimeoutMs { get; set; } = 5000;

        public int KeepAliveIntervalMs { get; set; } = 10000;

        public bool AutoReconnect { get; set; } = true;

        public int ReconnectBaseMs { get; set; } = 1000;

        public int ReconnectMaxMs { get; set; } = 30000;

        public MessageTerminator Terminator { get; set; } = MessageTerminator.Nul;

        public byte[] CustomTerminator { get; set; } = [];

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendBufferSize { get; set; } = 4096;
    }
}
