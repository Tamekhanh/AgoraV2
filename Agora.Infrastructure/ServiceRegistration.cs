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

        // Chọn EventBus dựa vào cấu hình: ưu tiên RabbitMQ nếu cấu hình bật
        var useRabbitMq = config.GetValue<bool>("UseRabbitMQ");
        if (useRabbitMq)
        {
            services.AddSingleton<IEventBus, RabbitMQEventBus>();
        }
        else
        {
            // Mặc định dùng InMemoryEventBus khi không có RabbitMQ
            services.AddSingleton<IEventBus, InMemoryEventBus>();
        }
        
        services.AddHostedService<OutboxWorker>();

        return services;
    }
}
