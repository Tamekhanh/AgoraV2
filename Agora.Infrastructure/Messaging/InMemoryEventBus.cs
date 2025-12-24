using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agora.Domain.Entities;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agora.Infrastructure.Messaging
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InMemoryEventBus> _logger;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly Dictionary<string, Type> _eventTypes;

        public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new Dictionary<string, Type>();
        }

        public async Task Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            _logger.LogInformation("Publishing event {EventName}", eventName);
            
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AgoraDbContext>();
                    var subscriptions = _handlers[eventName];
                    
                    foreach (var handlerType in subscriptions)
                    {
                        _logger.LogInformation("Dispatching to handler {HandlerType}", handlerType.Name);
                        var handler = scope.ServiceProvider.GetService(handlerType);
                        if (handler == null) 
                        {
                            _logger.LogWarning("Could not resolve handler {HandlerType}", handlerType.Name);
                            continue;
                        }

                        // Idempotency Check
                        var consumerName = handlerType.Name;
                        var messageId = @event.Id.ToString();

                        var alreadyProcessed = await dbContext.ProcessedMessages
                            .AnyAsync(pm => pm.MessageId == messageId && pm.ConsumerName == consumerName);

                        if (alreadyProcessed)
                        {
                            _logger.LogInformation("Message {MessageId} already processed by {ConsumerName}", messageId, consumerName);
                            continue;
                        }

                        var eventType = _eventTypes[eventName];
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        var method = concreteType.GetMethod("Handle");
                        
                        if (method != null)
                        {
                            try 
                            {
                                await (Task)method.Invoke(handler, new object[] { @event })!;
                                
                                // Save processed message
                                dbContext.ProcessedMessages.Add(new ProcessedMessage
                                {
                                    Id = Guid.NewGuid(),
                                    MessageId = messageId,
                                    ConsumerName = consumerName,
                                    ProcessedOn = DateTime.UtcNow
                                });
                                await dbContext.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error handling event {EventName} with {Handler}", eventName, consumerName);
                            }
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("No handlers found for event {EventName}", eventName);
            }
        }

        public Task Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);
            
            _logger.LogInformation("Subscribing {HandlerType} to {EventName}", handlerType.Name, eventName);

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
