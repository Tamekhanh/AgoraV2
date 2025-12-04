using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Agora.Infrastructure.Messaging
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _hostname = "localhost";
        private readonly string _exchangeName = "agora_event_bus";
        private readonly string _queueName = "agora_queue";
        
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly Dictionary<string, Type> _eventTypes;
        private bool _isDisposed;

        public RabbitMQEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                    var subscriptions = _handlers[eventName];
                    foreach (var handlerType in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(handlerType);
                        if (handler == null) continue;

                        var eventType = _eventTypes[eventName];
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                        if (integrationEvent == null) continue;

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        var method = concreteType.GetMethod("Handle");
                        if (method != null)
                        {
                            await (Task)method.Invoke(handler, new object[] { integrationEvent })!;
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
