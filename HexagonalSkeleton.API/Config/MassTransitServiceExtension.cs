using HexagonalSkeleton.Application.Services;
using HexagonalSkeleton.Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Simple MassTransit configuration with RabbitMQ
    /// </summary>
    public static class MassTransitServiceExtension
    {
        public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                // Register only essential consumers
                x.AddConsumer<UserCreatedConsumer>();
                x.AddConsumer<UserLoggedInConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost";
                    
                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username("hexagonal_user");
                        h.Password("hexagonal_password");
                    });

                    // Simple retry policy
                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(30)
                    ));

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Register the integration event service
            services.AddScoped<IIntegrationEventService, MassTransitIntegrationEventService>();

            return services;
        }
    }
}
