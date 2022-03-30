using Ddon.Socket.Connection;
using Ddon.Socket.Handler;

namespace Ddon.Socket
{
    public class DdonSocketStorage<TDdonSocketHandler> where TDdonSocketHandler : DdonSocketHandlerBase, new()
    {
        private static readonly object _lock = new();

        private readonly Dictionary<Guid, DdonSocketConnectionServer<TDdonSocketHandler>> Pairs = new();

        private static DdonSocketStorage<TDdonSocketHandler>? ddonSocketClientConnection;

        public IEnumerable<DdonSocketConnectionServer<TDdonSocketHandler>> Clients => Pairs.Values;

        internal static DdonSocketStorage<TDdonSocketHandler> GetInstance()
        {
            if (ddonSocketClientConnection != null) return ddonSocketClientConnection;

            lock (_lock) ddonSocketClientConnection ??= new DdonSocketStorage<TDdonSocketHandler>();

            return ddonSocketClientConnection;
        }

        public DdonSocketConnectionServer<TDdonSocketHandler>? GetClient(Guid SocketId)
        {
            return Pairs.ContainsKey(SocketId) ? Pairs[SocketId] : null;
        }

        public IEnumerable<DdonSocketConnectionServer<TDdonSocketHandler>>? GetClients(IEnumerable<Guid> SocketIds)
        {
            return Pairs.Values.Where(x => SocketIds.Contains(x.SocketId));
        }

        public void Add(DdonSocketConnectionServer<TDdonSocketHandler> client)
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
