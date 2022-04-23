using Ddon.Socket.Session;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Socket
{
    public class DdonSocketStorage
    {
        private static readonly object _lock = new();

        private readonly Dictionary<Guid, DdonSocketSession> Pairs = new();

        private static DdonSocketStorage? ddonSocketClientConnection;

        public IEnumerable<DdonSocketSession> Clients => Pairs.Values;

        public static DdonSocketStorage GetInstance()
        {
            if (ddonSocketClientConnection != null) return ddonSocketClientConnection;

            lock (_lock) ddonSocketClientConnection ??= new DdonSocketStorage();

            return ddonSocketClientConnection;
        }

        public DdonSocketSession? GetClient(Guid socketId)
        {
            return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
        }

        public IEnumerable<DdonSocketSession>? GetClients(IEnumerable<Guid> socketIds)
        {
            return Pairs.Values.Where(x => socketIds.Contains(x.Conn.SocketId));
        }

        public void Add(DdonSocketSession session)
        {
            if (!Pairs.ContainsKey(session.Conn.SocketId))
                Pairs.Add(session.Conn.SocketId, session);
        }

        public void Remove(Guid clientId)
        {
            if (Pairs.ContainsKey(clientId))
                Pairs.Remove(clientId);
        }
    }
}
