using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Agora.Infrastructure.Messaging
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly Dictionary<string, Type> _eventTypes;

        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new Dictionary<string, Type>();
        }

        public async Task Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _handlers[eventName];
                    foreach (var handlerType in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(handlerType);
                        if (handler == null) continue;

                        var eventType = _eventTypes[eventName];
                        // In memory, we don't need to deserialize, we have the object.
                        // But we need to ensure type safety if we were doing something generic.
                        // Here @event is IntegrationEvent, but the handler expects T : IntegrationEvent.
                        
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        var method = concreteType.GetMethod("Handle");
                        if (method != null)
                        {
                            await (Task)method.Invoke(handler, new object[] { @event })!;
                        }
                    }
                }
            }
        }

        public Task Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.ContainsKey(eventName))
            {
                _eventTypes.Add(eventName, typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (!_handlers[eventName].Contains(handlerType))
            {
                _handlers[eventName].Add(handlerType);
            }

            return Task.CompletedTask;
        }
    }
}
