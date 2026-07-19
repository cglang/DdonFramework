using System;
using System.Collections.Generic;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Configuration;
using Ddon.OpenProtocol.Core;
using Ddon.OpenProtocol.Models;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Configuration;

using Microsoft.Extensions.Logging;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Builder
{
    public class OpenProtocolEndpointBuilder
    {
        private readonly string _name;
        private readonly ISocketFactory _socketFactory;
        private readonly IServiceProvider? _serviceProvider;
        private readonly ILoggerFactory? _loggerFactory;
        private readonly OpenProtocolClientOptions _options = new();
        private readonly Dictionary<int, int> _responseMappings = new();
        private Type? _reconnectStrategyType;
        private MidInterpreter? _interpreter;

        internal OpenProtocolEndpointBuilder(
            string name,
            ISocketFactory socketFactory,
            IServiceProvider? serviceProvider = null,
            ILoggerFactory? loggerFactory = null)
        {
            _name = name;
            _socketFactory = socketFactory;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _options.Name = name;
        }

        public OpenProtocolEndpointBuilder Configure(Action<OpenProtocolClientOptions> configure)
        {
            configure(_options);
            return this;
        }

        public OpenProtocolEndpointBuilder MapResponse<TRequest, TResponse>()
            where TRequest : Mid
            where TResponse : Mid
        {
            int requestMid;
            int responseMid;

            try
            {
                requestMid = ((TRequest)Activator.CreateInstance(typeof(TRequest))!).Header.Mid;
                responseMid = ((TResponse)Activator.CreateInstance(typeof(TResponse))!).Header.Mid;
            }
            catch
            {
                var reqField = typeof(TRequest).GetField("MID");
                var resField = typeof(TResponse).GetField("MID");
                requestMid = (int)(reqField?.GetValue(null) ?? 0);
                responseMid = (int)(resField?.GetValue(null) ?? 0);
            }

            _responseMappings[requestMid] = responseMid;
            return this;
        }

        public OpenProtocolEndpointBuilder UseReconnect<TStrategy>()
            where TStrategy : Ddon.Socket.Abstractions.IReconnectStrategy
        {
            _reconnectStrategyType = typeof(TStrategy);
            return this;
        }

        public OpenProtocolEndpointBuilder UseInterpreter(MidInterpreter interpreter)
        {
            _interpreter = interpreter;
            return this;
        }

        internal IOpenProtocolEndpoint Build()
        {
            var socketOptions = new SocketClientOptions
            {
                Host = _options.Host,
                Port = _options.Port,
                ConnectTimeout = _options.ConnectTimeoutMs,
                NoDelay = true,
            };

            var worker = _socketFactory.CreateWorker(socketOptions);

            var interpreter = _interpreter
                ?? _serviceProvider?.GetService(typeof(MidInterpreter)) as MidInterpreter
                ?? new MidInterpreter().UseAllMessages();

            var eventBus = _serviceProvider?.GetService(typeof(OpenProtocolEventBus)) as OpenProtocolEventBus
                ?? new OpenProtocolEventBus();

            var logger = _loggerFactory?.CreateLogger<OpenProtocolEndpoint>()
                ?? (_serviceProvider?.GetService(typeof(ILogger<OpenProtocolEndpoint>)) as ILogger<OpenProtocolEndpoint>)
                ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<OpenProtocolEndpoint>.Instance;

            Ddon.Socket.Abstractions.IReconnectStrategy? reconnectStrategy = null;
            if (_reconnectStrategyType != null)
            {
                if (_serviceProvider != null)
                {
                    var service = _serviceProvider.GetService(_reconnectStrategyType);
                    if (service is Ddon.Socket.Abstractions.IReconnectStrategy s)
                    {
                        reconnectStrategy = s;
                    }
                }

                if (reconnectStrategy == null)
                {
                    var instance = Activator.CreateInstance(_reconnectStrategyType);
                    if (instance is Ddon.Socket.Abstractions.IReconnectStrategy s)
                    {
                        reconnectStrategy = s;
                    }
                }
            }
            else if (_options.AutoReconnect)
            {
                reconnectStrategy = new Ddon.Socket.Core.DefaultReconnectStrategy();
            }

            var endpoint = new OpenProtocolEndpoint(
                _name, _options, interpreter, eventBus, worker, logger, reconnectStrategy);

            foreach (var kvp in _responseMappings)
            {
                endpoint.MapResponse(kvp.Key, kvp.Value);
            }

            return endpoint;
        }
    }
}
