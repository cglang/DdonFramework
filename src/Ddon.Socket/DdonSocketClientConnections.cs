namespace DdonSocket
{
    public class DdonSocketClientConnections<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandlerCore, new()
    {
        private static readonly object _lock = new();

        private readonly Dictionary<Guid, DdonSocketClient<TDdonSocketHandler>> Pairs = new();

        private static DdonSocketClientConnections<TDdonSocketHandler>? ddonSocketClientConnection;

        public static DdonSocketClientConnections<TDdonSocketHandler> GetInstance()
        {
            if (ddonSocketClientConnection != null) return ddonSocketClientConnection;

            lock (_lock) ddonSocketClientConnection ??= new DdonSocketClientConnections<TDdonSocketHandler>();

            return ddonSocketClientConnection;
        }

        public DdonSocketClient<TDdonSocketHandler>? GetClient(Guid clientId)
        {
            return Pairs.ContainsKey(clientId) ? Pairs[clientId] : null;
        }

        public void Add(DdonSocketClient<TDdonSocketHandler> client)
        {
            Pairs.Add(client.ClientId, client);
        }

        public void Remove(Guid clientId)
        {
            if (Pairs.ContainsKey(clientId))
                Pairs.Remove(clientId);
        }
    }
}
