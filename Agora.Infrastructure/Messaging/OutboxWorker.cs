using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agora.Domain.Entities;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Agora.Infrastructure.Messaging
{
    public class OutboxWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxWorker> _logger;

        public OutboxWorker(IServiceProvider serviceProvider, ILogger<OutboxWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessages(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                }

                await Task.Delay(30000, stoppingToken); // Poll every 30 seconds
            }
        }

        private async Task ProcessOutboxMessages(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AgoraDbContext>();
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                var messages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedOn == null && m.ErrorTime < 10)
                    .OrderBy(m => m.OccurredOn)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        _logger.LogInformation("Processing outbox message {Id} of type {Type}", message.Id, message.Type);
                        var eventType = Type.GetType(message.Type);
                        if (eventType != null)
                        {
                            var integrationEvent = JsonConvert.DeserializeObject(message.Content, eventType) as IntegrationEvent;
                            if (integrationEvent != null)
                            {
                                await eventBus.Publish(integrationEvent);
                                _logger.LogInformation("Published event {Id} to EventBus", integrationEvent.Id);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to deserialize message content to IntegrationEvent");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Could not resolve type {Type}", message.Type);
                        }

                        message.ProcessedOn = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message;
                        message.ErrorTime++;
                        _logger.LogError(ex, "Failed to process outbox message {Id}. Retry count: {RetryCount}", message.Id, message.ErrorTime);

                        if (message.ErrorTime >= 10)
                        {
                            _logger.LogError("Message {Id} has reached maximum retry attempts and will be cancelled.", message.Id);
                        }
                    }
                }

                if (messages.Any())
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}
