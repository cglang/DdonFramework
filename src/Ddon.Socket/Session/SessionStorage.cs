using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Socket.Session
{
    internal class SessionStorage
    {
        private SessionStorage() { }

        public static SessionStorage Instance = new Lazy<SessionStorage>(() => new SessionStorage()).Value;

        private readonly Dictionary<Guid, SocketSession> Pairs = new();

        public IEnumerable<SocketSession> Sessions => Pairs.Values;


        public SocketSession? GetClient(Guid sessionId)
        {
            return Pairs.ContainsKey(sessionId) ? Pairs[sessionId] : null;
        }

        public IEnumerable<SocketSession>? GetClients(IEnumerable<Guid> sessionIds)
        {
            return Pairs.Values.Where(x => sessionIds.Contains(x.SessionId));
        }

        public void Add(SocketSession session)
        {
            if (!Pairs.ContainsKey(session.SessionId))
                Pairs.Add(session.SessionId, session);
        }

        public void Remove(Guid sessionId)
        {
            if (Pairs.ContainsKey(sessionId))
                Pairs.Remove(sessionId);
        }
    }
}
