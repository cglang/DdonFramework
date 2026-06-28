using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Builder;
using Ddon.Socket.Configuration;
using Microsoft.Extensions.Logging;

namespace Ddon.Socket.Core
{
    public class SocketServer
    {
        private readonly string _name;
        private readonly SocketServerOptions _options;
        private readonly Action<SocketEndpointBuilder> _configureEndpoint;
        private readonly ISocketManager _manager;
        private readonly IServiceProvider? _serviceProvider;
        private readonly ILogger? _logger;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _acceptLoop;

        public SocketServer(
            string name,
            SocketServerOptions options,
            Action<SocketEndpointBuilder> configureEndpoint,
            ISocketManager manager,
            IServiceProvider? serviceProvider = null,
            ILogger<SocketServer>? logger = null)
        {
            _name = name;
            _options = options;
            _configureEndpoint = configureEndpoint;
            _manager = manager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _listener = new TcpListener(_options.Address, _options.Port);
            _listener.Start(_options.Backlog);

            _logger?.LogInformation("Socket server '{Name}' listening on {Address}:{Port}", _name, _options.Address, _options.Port);

            _acceptLoop = AcceptLoopAsync(_cts.Token);
        }

        public async Task StopAsync()
        {
            _cts?.Cancel();

            try { _listener?.Stop(); } catch { }

            if (_acceptLoop != null)
            {
                try { await _acceptLoop; } catch { }
            }

            _logger?.LogInformation("Socket server '{Name}' stopped", _name);
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await _listener!.AcceptTcpClientAsync();
                    _ = HandleClientAsync(tcpClient, cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Accept loop error on server '{Name}'", _name);
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            var id = $"{_name}:{Guid.NewGuid():N}";

            try
            {
                var builder = new SocketEndpointBuilder(id, _serviceProvider);
                _configureEndpoint(builder);

                var endpoint = builder.Build(tcpClient);

                _manager.AddEndpoint(id, endpoint);
                await endpoint.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to handle client on server '{Name}'", _name);
                tcpClient.Dispose();
            }
        }
    }
}
