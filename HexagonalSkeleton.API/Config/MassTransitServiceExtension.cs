using HexagonalSkeleton.Application.Services;
using HexagonalSkeleton.Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// Extension methods for configuring MassTransit with RabbitMQ
    /// </summary>
    public static class MassTransitServiceExtension
    {
        public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                // Register all consumers
                x.AddConsumer<UserCreatedConsumer>();
                x.AddConsumer<UserProfileUpdatedConsumer>();
                x.AddConsumer<UserLoggedInConsumer>();
                x.AddConsumer<UserDeletedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = configuration.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost";
                    
                    cfg.Host(rabbitMqHost, h =>
                    {
                        h.Username("hexagonal_user");
                        h.Password("hexagonal_password");
                    });

                    // Configure retry policy
                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15),
                        TimeSpan.FromMinutes(1)
                    ));

                    // Configure endpoints for consumers
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Register the integration event service
            services.AddScoped<IIntegrationEventService, MassTransitIntegrationEventService>();

            return services;
        }
    }
}
