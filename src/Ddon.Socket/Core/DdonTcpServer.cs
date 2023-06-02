using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Core.Handle;

namespace Ddon.Socket.Core
{
    public class DdonTcpServer<THandler> where THandler : IDdonTcpClientDataHandler, new()
    {
        private readonly DdonTcpListener<THandler> _listener;
        private readonly ITcpConnecttionPool _connecttionPool;

        public DdonTcpServer(IPAddress ipAddress, int port, ITcpConnecttionPool connecttionPool)
        {
            _listener = new DdonTcpListener<THandler>(ipAddress, port);
            _connecttionPool = connecttionPool;
        }

        public void Start()
        {
            Task<Task>.Factory.StartNew(StartAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Task<Task>.Factory.StartNew(ProcessDataAsync, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task StartAsync()
        {
            _listener.Start();
            while (true)
            {
                var client = await _listener.AcceptDdonTcpClientAsync();
                _connecttionPool.Add(client);
            }
        }

        private async Task ProcessDataAsync()
        {
            while (true)
            {
                var client = await _connecttionPool.TakeAsync();
                _ = client.ProcessAsync();
            }
        }
    }
}
