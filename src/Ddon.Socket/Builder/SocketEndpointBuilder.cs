using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Ddon.Pipeline;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Configuration;
using Ddon.Socket.Core;
using Ddon.Socket.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Socket.Builder
{
    public class SocketEndpointBuilder
    {
        private readonly string _name;
        private readonly IServiceProvider? _serviceProvider;
        private readonly SocketClientOptions _options = new SocketClientOptions();
        private readonly List<ISocketHandler> _handlers = new List<ISocketHandler>();
        private Type? _protocolType;
        private Type? _reconnectStrategyType;
        private ISocketPipeline? _serialPipeline;

        internal SocketEndpointBuilder(string name, IServiceProvider? serviceProvider)
        {
            _name = name;
            _serviceProvider = serviceProvider;
        }

        public SocketEndpointBuilder Configure(Action<SocketClientOptions> configure)
        {
            configure(_options);
            return this;
        }

        public SocketEndpointBuilder UseProtocol<TProtocol>() where TProtocol : ISocketProtocol
        {
            _protocolType = typeof(TProtocol);
            return this;
        }

        public SocketEndpointBuilder UseReconnect<TStrategy>() where TStrategy : IReconnectStrategy
        {
            _reconnectStrategyType = typeof(TStrategy);
            return this;
        }

        public SocketEndpointBuilder UsePipeline(Action<PipelineBuilder> configure)
        {
            var pipelineBuilder = new PipelineBuilder();
            configure(pipelineBuilder);
            var pipeline = pipelineBuilder.Build();

            _serialPipeline = new SocketPipeline(pipeline);
            return this;
        }

        public SocketEndpointBuilder AddHandler<THandler>() where THandler : ISocketHandler
        {
            if (_serviceProvider != null)
            {
                var handler = _serviceProvider.GetService(typeof(THandler));
                if (handler is ISocketHandler h)
                {
                    _handlers.Add(h);
                    return this;
                }
            }

            var instance = Activator.CreateInstance<THandler>();
            _handlers.Add(instance);
            return this;
        }

        public void AddHandlerInstance(ISocketHandler handler)
        {
            _handlers.Add(handler);
        }

        internal ISocketEndpoint Build()
        {
            var factory = new SocketFactory(_serviceProvider);
            var worker = factory.CreateWorker(_options);

            ISocketProtocol? protocol = null;
            if (_protocolType != null)
                protocol = factory.CreateProtocol(_protocolType);

            IReconnectStrategy? reconnectStrategy = null;
            if (_reconnectStrategyType != null)
                reconnectStrategy = factory.CreateReconnectStrategy(_reconnectStrategyType);

            var pipeline = _serialPipeline ?? BuildDefaultPipeline();

            var dispatcher = new SocketDispatcher(_handlers);

            return new SocketEndpoint(_name, worker, pipeline, dispatcher, protocol, reconnectStrategy);
        }

        internal ISocketEndpoint Build(TcpClient acceptedClient)
        {
            var factory = new SocketFactory(_serviceProvider);

            var options = new SocketClientOptions
            {
                NoDelay = _options.NoDelay,
                ReceiveBufferSize = _options.ReceiveBufferSize,
                SendBufferSize = _options.SendBufferSize,
            };

            var worker = factory.CreateWorker(acceptedClient, options);

            ISocketProtocol? protocol = null;
            if (_protocolType != null)
                protocol = factory.CreateProtocol(_protocolType);

            IReconnectStrategy? reconnectStrategy = null;
            if (_reconnectStrategyType != null)
                reconnectStrategy = factory.CreateReconnectStrategy(_reconnectStrategyType);

            var pipeline = _serialPipeline ?? BuildDefaultPipeline();

            var dispatcher = new SocketDispatcher(_handlers);

            return new SocketEndpoint(_name, worker, pipeline, dispatcher, protocol, reconnectStrategy);
        }

        private ISocketPipeline BuildDefaultPipeline()
        {
            var build = GeneralCustomPipelineFactory<SocketContext>.CreatePipelineBuild();
            return new SocketPipeline(build.Build());
        }
    }
}
