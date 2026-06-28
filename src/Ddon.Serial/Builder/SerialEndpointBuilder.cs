using System;
using System.Collections.Generic;
using Ddon.Pipeline;
using Ddon.Serial.Abstractions;
using Ddon.Serial.Configuration;
using Ddon.Serial.Core;
using Ddon.Serial.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Serial.Builder
{
    public class SerialEndpointBuilder
    {
        private readonly string _name;
        private readonly IServiceProvider? _serviceProvider;
        private readonly SerialPortOptions _options = new SerialPortOptions();
        private readonly List<ISerialHandler> _handlers = new List<ISerialHandler>();
        private readonly List<Action<PipelineRegistrar<SerialContext>>> _middlewareActions = new List<Action<PipelineRegistrar<SerialContext>>>();
        private Type? _protocolType;
        private Type? _reconnectStrategyType;

        internal SerialEndpointBuilder(string name, IServiceProvider? serviceProvider)
        {
            _name = name;
            _serviceProvider = serviceProvider;
        }

        public SerialEndpointBuilder Configure(Action<SerialPortOptions> configure)
        {
            configure(_options);
            return this;
        }

        public SerialEndpointBuilder UseProtocol<TProtocol>() where TProtocol : ISerialProtocol
        {
            _protocolType = typeof(TProtocol);
            return this;
        }

        public SerialEndpointBuilder UseReconnect<TStrategy>() where TStrategy : IReconnectStrategy
        {
            _reconnectStrategyType = typeof(TStrategy);
            return this;
        }

        public SerialEndpointBuilder UsePipeline(Action<PipelineBuilder> configure)
        {
            var pipelineBuilder = new PipelineBuilder();
            configure(pipelineBuilder);
            var pipeline = pipelineBuilder.Build();

            var serialPipeline = new SerialPipeline(pipeline);

            _serialPipeline = serialPipeline;
            return this;
        }

        public SerialEndpointBuilder AddHandler<THandler>() where THandler : ISerialHandler
        {
            if (_serviceProvider != null)
            {
                var handler = _serviceProvider.GetService(typeof(THandler));
                if (handler is ISerialHandler h)
                {
                    _handlers.Add(h);
                    return this;
                }
            }

            var instance = Activator.CreateInstance<THandler>();
            _handlers.Add(instance);
            return this;
        }

        public void AddHandlerInstance(ISerialHandler handler)
        {
            _handlers.Add(handler);
        }

        internal void AddMiddlewareAction(Action<PipelineRegistrar<SerialContext>> action)
        {
            _middlewareActions.Add(action);
        }

        private ISerialPipeline? _serialPipeline;

        internal ISerialEndpoint Build()
        {
            var factory = new SerialFactory(_serviceProvider);
            var worker = factory.CreateWorker(_options);

            ISerialProtocol? protocol = null;
            if (_protocolType != null)
                protocol = factory.CreateProtocol(_protocolType);

            IReconnectStrategy? reconnectStrategy = null;
            if (_reconnectStrategyType != null)
                reconnectStrategy = factory.CreateReconnectStrategy(_reconnectStrategyType);

            var pipeline = _serialPipeline ?? BuildDefaultPipeline();

            var dispatcher = new SerialDispatcher(_handlers);

            return new SerialEndpoint(_name, _options, worker, pipeline, dispatcher, protocol, reconnectStrategy);
        }

        private ISerialPipeline BuildDefaultPipeline()
        {
            var build = GeneralCustomPipelineFactory<SerialContext>.CreatePipelineBuild();
            foreach (var action in _middlewareActions)
            {
                build.ConfigureMiddlewares(action);
            }
            return new SerialPipeline(build.Build());
        }
    }
}
