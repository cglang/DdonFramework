using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ddon.Socket.Core;

namespace Ddon.SimpeSocket.Core.Storage
{
    internal class SocketSessionStorage : ISocketSessionStorage
    {
        private static readonly ConcurrentDictionary<Guid, SocketSession> Pairs = new ConcurrentDictionary<Guid, SocketSession>();

        public SocketSession Get(Guid socketId)
        {
            return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
        }

        public IEnumerable<SocketSession> Get(IEnumerable<Guid> socketIds)
        {
            return Pairs.Values.Where(x => socketIds.Contains(x.SessionId));
        }

        public IEnumerable<SocketSession> GetAll()
        {
            return Pairs.Values;
        }

        public bool Add(SocketSession session)
        {
            return !Pairs.ContainsKey(session.SessionId) && Pairs.TryAdd(session.SessionId, session);
        }

        public void Remove(Guid clientId)
        {
            if (Pairs.ContainsKey(clientId))
                Pairs.TryRemove(clientId, out _);
        }
    }
}
