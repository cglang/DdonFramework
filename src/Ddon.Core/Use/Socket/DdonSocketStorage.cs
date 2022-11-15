using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Core.Use.Socket
{
    public static class DdonSocketStorage
    {
        private static readonly ConcurrentDictionary<Guid, DdonSocketCore> Pairs = new();

        public static IEnumerable<DdonSocketCore> Clients => Pairs.Values;

        public static DdonSocketCore? GetClient(Guid socketId)
        {
            return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
        }

        public static IEnumerable<DdonSocketCore> GetClients(IEnumerable<Guid> socketIds)
        {
            return Pairs.Values.Where(x => socketIds.Contains(x.SocketId));
        }

        public static bool Add(DdonSocketCore session)
        {
            if (!Pairs.ContainsKey(session.SocketId))
                return Pairs.TryAdd(session.SocketId, session);
            return false;
        }

        public static void Remove(Guid clientId)
        {
            if (Pairs.ContainsKey(clientId))
                Pairs.Remove(clientId, out _);
        }
    }
}
