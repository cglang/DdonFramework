using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Socket
{
    public class DdonSocketStorage
    {
        private static readonly object _lock = new();

        private readonly Dictionary<Guid, DdonSocketConnection> Pairs = new();

        private static DdonSocketStorage? ddonSocketClientConnection;

        public IEnumerable<DdonSocketConnection> Clients => Pairs.Values;

        public static DdonSocketStorage GetInstance()
        {
            if (ddonSocketClientConnection != null) return ddonSocketClientConnection;

            lock (_lock) ddonSocketClientConnection ??= new DdonSocketStorage();

            return ddonSocketClientConnection;
        }

        public DdonSocketConnection? GetClient(Guid SocketId)
        {
            return Pairs.ContainsKey(SocketId) ? Pairs[SocketId] : null;
        }

        public IEnumerable<DdonSocketConnection>? GetClients(IEnumerable<Guid> SocketIds)
        {
            return Pairs.Values.Where(x => SocketIds.Contains(x.SocketId));
        }

        public void Add(DdonSocketConnection client)
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
