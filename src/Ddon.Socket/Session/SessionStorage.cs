using System;
using System.Collections.Generic;
using System.Linq;
using Ddon.Core.Use.Socket;

namespace Ddon.Socket.Session
{
    internal class SessionStorage
    {
        private SessionStorage() { }

        public static SessionStorage Instance = new Lazy<SessionStorage>(() => new SessionStorage()).Value;

        private readonly Dictionary<Guid, SocketCoreSession> Pairs = new();

        public IEnumerable<SocketCoreSession> Sessions => Pairs.Values;


        public SocketCoreSession? GetClient(Guid sessionId)
        {
            return Pairs.ContainsKey(sessionId) ? Pairs[sessionId] : null;
        }

        public IEnumerable<SocketCoreSession>? GetClients(IEnumerable<Guid> sessionIds)
        {
            return Pairs.Values.Where(x => sessionIds.Contains(x.SessionId));
        }

        public void Add(SocketCoreSession session)
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
