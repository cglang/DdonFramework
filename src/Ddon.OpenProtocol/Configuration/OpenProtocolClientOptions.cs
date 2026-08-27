namespace Ddon.OpenProtocol.Configuration
{
    public class OpenProtocolClientOptions
    {
        public string Name { get; set; } = "default";

        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 4545;

        public int ConnectTimeoutMs { get; set; } = 5_000;

        public int RequestTimeoutMs { get; set; } = 5_000;

        public int KeepAliveIntervalMs { get; set; } = 10_000;

        public bool AutoReconnect { get; set; } = true;

        public int ReconnectBaseMs { get; set; } = 1_000;

        public int ReconnectMaxMs { get; set; } = 30_000;

        public MessageTerminator Terminator { get; set; } = MessageTerminator.Nul;

        public string CustomTerminator { get; set; } = "\0";

        public int ReceiveBufferSize { get; set; } = 4096;
    }
}
