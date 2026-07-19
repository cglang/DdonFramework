namespace Ddon.OpenProtocol.Configuration
{
    public class OpenProtocolClientOptions
    {
        public string Name { get; set; } = "default";

        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 4545;

        public int ConnectTimeoutMs { get; set; } = 500_000;

        public int RequestTimeoutMs { get; set; } = 5_000;

        public int KeepAliveIntervalMs { get; set; } = 10_000;

        public int ReconnectBaseMs { get; set; } = 1_000;

        public int ReconnectMaxMs { get; set; } = 30_000;

        public bool AutoReconnect { get; set; } = true;

        public MessageTerminator Terminator { get; set; } = MessageTerminator.Nul;
    }
}
