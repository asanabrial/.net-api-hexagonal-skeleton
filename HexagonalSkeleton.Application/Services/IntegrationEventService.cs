using HexagonalSkeleton.Application.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Application.Services
{
    /// <summary>
    /// Service for publishing integration events
    /// Abstracts the message bus implementation
    /// </summary>
    public interface IIntegrationEventService
    {
        Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
            where T : class, IIntegrationEvent;
    }

    /// <summary>
    /// MassTransit-based implementation of integration event service
    /// Provides reliable message delivery through RabbitMQ with automatic retries
    /// Handles serialization, routing, and error handling automatically
    /// </summary>
    public class MassTransitIntegrationEventService : IIntegrationEventService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<MassTransitIntegrationEventService> _logger;

        public MassTransitIntegrationEventService(
            IPublishEndpoint publishEndpoint,
            ILogger<MassTransitIntegrationEventService> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
            where T : class, IIntegrationEvent
        {
            try
            {
                _logger.LogInformation("Publishing integration event {EventType} with ID {EventId} via MassTransit", 
                    integrationEvent.EventType, integrationEvent.EventId);

                // MassTransit handles serialization, routing, retries, and error handling
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                _logger.LogInformation("Successfully published integration event {EventType} with ID {EventId}", 
                    integrationEvent.EventType, integrationEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish integration event {EventType} with ID {EventId}", 
                    integrationEvent.EventType, integrationEvent.EventId);
                throw;
            }
        }
    }

    /// <summary>
    /// Handler interface for integration events compatible with MassTransit
    /// </summary>
    public interface IIntegrationEventHandler<in T> : IConsumer<T> 
        where T : class, IIntegrationEvent
    {
        // MassTransit will automatically call the Consume method
        // We inherit from IConsumer<T> to leverage MassTransit's consumer infrastructure
    }
}
