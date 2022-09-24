using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Core.Use.Socket
{
    public static class DdonSocketStorage
    {
        private static readonly Dictionary<Guid, DdonSocketCore> Pairs = new();

        public static IEnumerable<DdonSocketCore> Clients => Pairs.Values;

        public static DdonSocketCore? GetClient(Guid socketId)
        {
            return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
        }

        public static IEnumerable<DdonSocketCore> GetClients(IEnumerable<Guid> socketIds)
        {
            return Pairs.Values.Where(x => socketIds.Contains(x.SocketId));
        }

        public static void Add(DdonSocketCore session)
        {
            if (!Pairs.ContainsKey(session.SocketId))
                Pairs.Add(session.SocketId, session);
        }

        public static void Remove(Guid clientId)
        {
            if (Pairs.ContainsKey(clientId))
                Pairs.Remove(clientId);
        }
    }
}
