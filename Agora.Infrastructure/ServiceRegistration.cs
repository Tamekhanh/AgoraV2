using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Agora.Infrastructure.Messaging;
using Agora.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agora.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AgoraDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddTransient<IEmailService, EmailService>();
        
        // Use InMemoryEventBus for development if RabbitMQ is not available
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        // services.AddSingleton<IEventBus, RabbitMQEventBus>();
        
        services.AddHostedService<OutboxWorker>();

        return services;
    }
}
