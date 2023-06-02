using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddon.Socket.Core
{
    public class TcpConnectionPool : ITcpConnecttionPool
    {
        private static Dictionary<Guid, IDdonTcpClient> Clients => new();

        public bool Remove(Guid id)
        {
            return Clients.Remove(id);
        }

        public void Add(IDdonTcpClient tcpClient)
        {
            Clients.Add(tcpClient.Id, tcpClient);
        }

        public async Task<IDdonTcpClient> TakeAsync()
        {
            while (true)
            {
                foreach (var client in Clients)
                {
                    if (client.Value.CanRead) return client.Value;
                }
                await Task.Delay(100);
            }
        }
    }
}
