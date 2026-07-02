using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Ddon.Common.EventBus;
using Ddon.EventBus.Contracts;
using Ddon.EventBus.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventBusInMemoryExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, Assembly assembly)
        {
            var registrations = DiscoverHandlers(assembly);

            foreach (var reg in registrations)
                services.AddTransient(reg.HandlerType);

            services.AddSingleton<IEventBus>(sp =>
            {
                SubscribeHandlers(sp, registrations);
                return new InMemoryEventBus();
            });

            return services;
        }

        private static List<HandlerRegistration> DiscoverHandlers(Assembly assembly)
        {
            var result = new List<HandlerRegistration>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType)
                        continue;

                    var def = iface.GetGenericTypeDefinition();
                    if (def == typeof(IEventHandler<>))
                    {
                        result.Add(new HandlerRegistration
                        {
                            HandlerType = type,
                            EventType = iface.GetGenericArguments()[0],
                            IsDomainHandler = false
                        });
                    }
                    else if (def == typeof(IDomainEventHandler<>))
                    {
                        result.Add(new HandlerRegistration
                        {
                            HandlerType = type,
                            EventType = iface.GetGenericArguments()[0],
                            IsDomainHandler = true
                        });
                    }
                }
            }

            return result;
        }

        private static void SubscribeHandlers(IServiceProvider sp, List<HandlerRegistration> registrations)
        {
            var eventHandlerMethod = typeof(EventBusInMemoryExtensions)
                .GetMethod(nameof(SubscribeEventHandler), BindingFlags.NonPublic | BindingFlags.Static);
            var domainHandlerMethod = typeof(EventBusInMemoryExtensions)
                .GetMethod(nameof(SubscribeDomainHandler), BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var reg in registrations)
            {
                var method = reg.IsDomainHandler ? domainHandlerMethod : eventHandlerMethod;
                method.MakeGenericMethod(reg.EventType)
                    .Invoke(null, new object[] { reg.HandlerType, sp });
            }
        }

        private static void SubscribeEventHandler<TEvent>(Type handlerType, IServiceProvider sp)
            where TEvent : IEventData
        {
            GeneralEventBus.Default.Subscribe<TEvent>(evt =>
            {
                var handler = (IEventHandler<TEvent>)sp.GetRequiredService(handlerType);
                return handler.HandleAsync(evt, CancellationToken.None);
            }, ImmediateScheduler.Instance);
        }

        private static void SubscribeDomainHandler<TDomainEvent>(Type handlerType, IServiceProvider sp)
            where TDomainEvent : IDomainEventData
        {
            GeneralEventBus.Default.Subscribe<TDomainEvent>(evt =>
            {
                var handler = (IDomainEventHandler<TDomainEvent>)sp.GetRequiredService(handlerType);
                return handler.HandleAsync(evt, CancellationToken.None);
            }, ImmediateScheduler.Instance);
        }

        private sealed class HandlerRegistration
        {
            public Type HandlerType { get; set; }
            public Type EventType { get; set; }
            public bool IsDomainHandler { get; set; }
        }
    }
}
