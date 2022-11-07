using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Socket.Session
{
    internal class DdonSocketSessionStorage
    {
        private DdonSocketSessionStorage() { }

        public static DdonSocketSessionStorage Instance = new Lazy<DdonSocketSessionStorage>(() => new DdonSocketSessionStorage()).Value;

        private readonly Dictionary<Guid, SocketSession> Pairs = new();

        public IEnumerable<SocketSession> Clients => Pairs.Values;


        public SocketSession? GetClient(Guid socketId)
        {
            return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
        }

        public IEnumerable<SocketSession>? GetClients(IEnumerable<Guid> socketIds)
        {
            return Pairs.Values.Where(x => socketIds.Contains(x.Conn.SocketId));
        }

        public void Add(SocketSession session)
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
