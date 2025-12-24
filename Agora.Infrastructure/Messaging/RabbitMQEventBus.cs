using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Agora.Domain.Entities;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agora.Infrastructure.Messaging
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQEventBus> _logger;
        private readonly string _hostname;
        private readonly string _exchangeName = "agora_event_bus";
        private readonly string _queueName = "agora_queue";
        
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly Dictionary<string, Type> _eventTypes;
        private bool _isDisposed;

        public RabbitMQEventBus(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<RabbitMQEventBus> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hostname = configuration["RabbitMQ:HostName"] ?? "localhost";
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new Dictionary<string, Type>();
        }

        private async Task EnsureConnection()
        {
            if (_connection != null) return;

            var factory = new ConnectionFactory { HostName = _hostname };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Direct);
            await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += Consumer_ReceivedAsync;
            
            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
        }

        public async Task Publish(IntegrationEvent @event)
        {
            await EnsureConnection();

            var eventName = @event.GetType().Name;
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            if (_channel != null)
            {
                await _channel.BasicPublishAsync(exchange: _exchangeName, routingKey: eventName, body: body);
            }
        }

        public async Task Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            await EnsureConnection();

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
                if (_channel != null)
                {
                    await _channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: eventName);
                }
            }
        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body.ToArray());

            await ProcessEvent(eventName, message);
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AgoraDbContext>();
                    var subscriptions = _handlers[eventName];
                    
                    foreach (var handlerType in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(handlerType);
                        if (handler == null) continue;

                        var eventType = _eventTypes[eventName];
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType) as IntegrationEvent;
                        if (integrationEvent == null) continue;

                        // Idempotency Check
                        var consumerName = handlerType.Name;
                        var messageId = integrationEvent.Id.ToString();

                        var alreadyProcessed = await dbContext.ProcessedMessages
                            .AnyAsync(pm => pm.MessageId == messageId && pm.ConsumerName == consumerName);

                        if (alreadyProcessed)
                        {
                            _logger.LogInformation("Message {MessageId} already processed by {ConsumerName}", messageId, consumerName);
                            continue;
                        }

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        var method = concreteType.GetMethod("Handle");
                        if (method != null)
                        {
                            try 
                            {
                                await (Task)method.Invoke(handler, new object[] { integrationEvent })!;
                                
                                // Save processed message
                                dbContext.ProcessedMessages.Add(new ProcessedMessage
                                {
                                    MessageId = messageId,
                                    ConsumerName = consumerName,
                                    ProcessedOn = DateTime.UtcNow
                                });
                                await dbContext.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error handling event {EventName} with {Handler}", eventName, consumerName);
                                // Optionally handle retry logic here or let it fail to DLQ (if configured)
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            try
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _connection?.CloseAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore errors during dispose
            }
            _isDisposed = true;
        }
    }
}
