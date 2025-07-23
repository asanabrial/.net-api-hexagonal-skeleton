using HexagonalSkeleton.Application.IntegrationEvents;
using HexagonalSkeleton.Application.Services;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HexagonalSkeleton.Test;

namespace HexagonalSkeleton.Test.TestInfrastructure.Mocks
{
    /// <summary>
    /// Mock integration event service that synchronizes CQRS state immediately for testing
    /// Replaces real MassTransit event service to ensure immediate consistency
    /// </summary>
    public class MockIntegrationEventService : IIntegrationEventService
    {
        private readonly ILogger<MockIntegrationEventService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MockIntegrationEventService(
            ILogger<MockIntegrationEventService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : class, IIntegrationEvent
        {
            _logger.LogInformation("Mock: Publishing integration event {EventType} immediately", typeof(TEvent).Name);

            // Handle UserCreatedIntegrationEvent immediately for CQRS synchronization
            if (integrationEvent is UserCreatedIntegrationEvent userCreatedEvent)
            {
                await HandleUserCreatedEventAsync(userCreatedEvent, cancellationToken);
            }
            else if (integrationEvent is UserLoggedInIntegrationEvent userLoggedInEvent)
            {
                await HandleUserLoggedInEventAsync(userLoggedInEvent, cancellationToken);
            }
            
            _logger.LogInformation("Mock: Successfully processed integration event {EventType}", typeof(TEvent).Name);
        }

        private async Task HandleUserCreatedEventAsync(UserCreatedIntegrationEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mock: Processing UserCreated event for user {UserId} immediately", @event.UserId);
            
            // Get the user from write repository to synchronize to read repository
            var writeRepository = _serviceProvider.GetService<IUserWriteRepository>();
            if (writeRepository != null)
            {
                try
                {
                    // Get the actual user from write repository
                    var existingUser = await writeRepository.GetUserByEmailAsync(@event.Email, cancellationToken);
                    if (existingUser != null)
                    {
                        _logger.LogInformation("Mock: User {UserId} with email {Email} exists in write repository - synchronizing to read repository", @event.UserId, @event.Email);
                        
                        // Synchronize directly to the shared InMemoryUserReadRepository
                        HexagonalSkeleton.Test.InMemoryUserReadRepository.SynchronizeUser(existingUser);
                        _logger.LogInformation("Mock: Successfully synchronized user {UserId} to read repository", @event.UserId);
                    }
                    else
                    {
                        _logger.LogWarning("Mock: User {UserId} with email {Email} not found in write repository after creation", @event.UserId, @event.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Mock: Failed to synchronize user {UserId} to read repository", @event.UserId);
                }
            }
            else
            {
                _logger.LogWarning("Mock: IUserWriteRepository not found in DI container");
            }
        }

        private Task HandleUserLoggedInEventAsync(UserLoggedInIntegrationEvent @event, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mock: Processing UserLoggedIn event for user {UserId} immediately", @event.UserId);
            
            // Mock implementation - just log the event
            // In real scenarios, this would update read models for login tracking
            
            return Task.CompletedTask;
        }
    }
}
