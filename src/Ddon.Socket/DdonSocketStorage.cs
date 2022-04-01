using Ddon.Socket.Connection;

namespace Ddon.ConvenientSocket
{
    public class DdonSocketStorage
    {
        private static readonly object _lock = new();

        private readonly Dictionary<Guid, DdonSocketConnectionCore> Pairs = new();

        private static DdonSocketStorage? ddonSocketClientConnection;

        public IEnumerable<DdonSocketConnectionCore> Clients => Pairs.Values;

        internal static DdonSocketStorage GetInstance()
        {
            if (ddonSocketClientConnection != null) return ddonSocketClientConnection;

            lock (_lock) ddonSocketClientConnection ??= new DdonSocketStorage();

            return ddonSocketClientConnection;
        }

        public DdonSocketConnectionCore? GetClient(Guid SocketId)
        {
            return Pairs.ContainsKey(SocketId) ? Pairs[SocketId] : null;
        }

        public IEnumerable<DdonSocketConnectionCore>? GetClients(IEnumerable<Guid> SocketIds)
        {
            return Pairs.Values.Where(x => SocketIds.Contains(x.SocketId));
        }

        public void Add(DdonSocketConnectionCore client)
        {
            if (!Pairs.ContainsKey(client.SocketId))
                Pairs.Add(client.SocketId, client);
        }

        public void Remove(Guid clientId)
        {
            if (Pairs.ContainsKey(clientId))
                Pairs.Remove(clientId);
        }
    }
}
