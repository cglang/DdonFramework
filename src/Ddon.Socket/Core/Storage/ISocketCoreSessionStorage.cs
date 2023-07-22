using System;
using System.Collections.Generic;

namespace Ddon.Socket.Core.Storage
{
    public interface ISocketCoreSessionStorage
    {
        SocketCoreSession? Get(Guid socketId);

        IEnumerable<SocketCoreSession> Get(IEnumerable<Guid> socketIds);

        IEnumerable<SocketCoreSession> GetAll();

        internal bool Add(SocketCoreSession session);

        internal void Remove(Guid clientId);
    }
}
