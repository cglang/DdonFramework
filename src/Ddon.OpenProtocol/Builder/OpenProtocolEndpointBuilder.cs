using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Abstractions;
using Ddon.OpenProtocol.Configuration;
using Ddon.OpenProtocol.Core;
using Ddon.OpenProtocol.Models;
using Ddon.Pipeline;
using Ddon.Socket.Abstractions;
using Ddon.Socket.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly OpenProtocolEndpointOptions _options = new();
        private readonly List<IOpenProtocolHandler> _handlers = new List<IOpenProtocolHandler>();
        private readonly Dictionary<int, Type> _customMids = new Dictionary<int, Type>();
        private MidInterpreter? _interpreter;
        private ISocketWorker? _customWorker;
        private OpenProtocolPipeline? _pipeline;

        private bool _built;

        internal OpenProtocolEndpointBuilder(
            string name,
            ISocketFactory socketFactory,
            IServiceProvider? serviceProvider,
            ILoggerFactory? loggerFactory)
        {
            _name = name;
            _socketFactory = socketFactory;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _options.Name = name;
        }

        public OpenProtocolEndpointBuilder Configure(Action<OpenProtocolEndpointOptions> configure)
        {
            configure(_options);
            return this;
        }

        public OpenProtocolEndpointBuilder UseInterpreter(MidInterpreter interpreter)
        {
            _interpreter = interpreter;
            return this;
        }

        public OpenProtocolEndpointBuilder UseInterpreter(Action<MidInterpreter> configure)
        {
            var interpreter = new MidInterpreter();
            configure(interpreter);
            _interpreter = interpreter;
            return this;
        }

        public OpenProtocolEndpointBuilder UseCustomWorker(ISocketWorker worker)
        {
            _customWorker = worker;
            return this;
        }

        public OpenProtocolEndpointBuilder UsePipeline(Action<PipelineBuilder> configure)
        {
            var build = GeneralCustomPipelineFactory<OpenProtocolContext>.CreatePipelineBuild();

            var pipelineBuilder = new PipelineBuilder(build);
            configure(pipelineBuilder);

            _pipeline = new OpenProtocolPipeline(build.Build());
            return this;
        }

        public OpenProtocolEndpointBuilder AddHandler<THandler>()
            where THandler : IOpenProtocolHandler
        {
            if (_serviceProvider is not null)
            {
                var handler = _serviceProvider.GetService(typeof(THandler));
                if (handler is IOpenProtocolHandler h)
                {
                    _handlers.Add(h);
                    return this;
                }
            }

            var instance = Activator.CreateInstance<THandler>();
            _handlers.Add(instance);
            return this;
        }

        public OpenProtocolEndpointBuilder RegisterCustomMid<T>(int? midNumber = null)
            where T : Mid
        {
            int mid = midNumber ?? OpenProtocolProtocol.GetMidFromType<T>();
            _customMids[mid] = typeof(T);
            return this;
        }

        public OpenProtocolEndpointBuilder MapResponse<TRequest, TResponse>()
            where TRequest : Mid
            where TResponse : Mid
        {
            int reqMid = OpenProtocolProtocol.GetMidFromType<TRequest>();
            int resMid = OpenProtocolProtocol.GetMidFromType<TResponse>();
            _responseMappings[reqMid] = resMid;
            return this;
        }

        public OpenProtocolEndpointBuilder MapResponse(int requestMid, int responseMid)
        {
            _responseMappings[requestMid] = responseMid;
            return this;
        }

        private readonly Dictionary<int, int> _responseMappings = new();

        internal IOpenProtocolEndpoint Build()
        {
            if (_built)
                throw new InvalidOperationException("Builder already used.");
            _built = true;

            var logger = _loggerFactory?.CreateLogger<OpenProtocolEndpoint>();

            ISocketWorker worker = _customWorker ?? CreateDefaultWorker();

            var interpreter = _interpreter ?? BuildDefaultInterpreter();

            if (_customMids.Count > 0)
            {
                interpreter.UseCustomMessage(new Dictionary<int, Type>(_customMids));
            }

            var protocol = new OpenProtocolProtocol(
                _options, interpreter,
                _loggerFactory?.CreateLogger<OpenProtocolProtocol>());

            foreach (var kvp in _responseMappings)
            {
                protocol.TryMapResponse(kvp.Key, kvp.Value);
            }

            var matcher = new RequestResponseMatcher(
                requestMid =>
                {
                    if (protocol.TryGetResponseMid(requestMid, out int responseMid))
                        return responseMid;
                    return null;
                },
                _loggerFactory?.CreateLogger<RequestResponseMatcher>());

            var eventBus = new OpenProtocolEventBus();

            OpenProtocolDispatcher? dispatcher = null;
            if (_handlers.Count > 0)
                dispatcher = new OpenProtocolDispatcher(_handlers);

            return new OpenProtocolEndpoint(
                worker, protocol, matcher, eventBus, _options, logger,
                _pipeline, dispatcher);
        }

        private ISocketWorker CreateDefaultWorker()
        {
            var socketOptions = new SocketClientOptions
            {
                Host = _options.Host,
                Port = _options.Port,
                ConnectTimeout = _options.ConnectTimeoutMs,
                ReceiveBufferSize = _options.ReceiveBufferSize,
                SendBufferSize = _options.SendBufferSize,
                NoDelay = true,
            };

            return _socketFactory.CreateWorker(socketOptions);
        }

        private static MidInterpreter BuildDefaultInterpreter()
        {
            return new MidInterpreter()
                .UseCustomMessage(new Dictionary<int, Type>
                {
                })
                .UseAllMessages();
        }

        private void EnsureInterpreter()
        {
            if (_interpreter is null)
                _interpreter = BuildDefaultInterpreter();
        }

        public sealed class PipelineBuilder
        {
            private readonly GeneralCustomPipelineBuild<OpenProtocolContext> _build;

            internal PipelineBuilder(GeneralCustomPipelineBuild<OpenProtocolContext> build)
            {
                _build = build;
            }

            public PipelineBuilder Use<TMiddleware>()
                where TMiddleware : IOpenProtocolMiddleware
            {
                _build.ConfigureMiddlewares(registrar =>
                {
                    registrar.AddMiddleware<TMiddleware>();
                });
                return this;
            }

            public PipelineBuilder Use(Func<OpenProtocolContext, Task> action)
            {
                _build.ConfigureMiddlewares(registrar =>
                {
                    registrar.AddMiddleware(action);
                });
                return this;
            }
        }
    }
}
