using HexagonalSkeleton.Application.IntegrationEvents;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Test.TestInfrastructure.Mocks
{
    /// <summary>
    /// Test mock for UserCreatedConsumer that synchronizes with InMemoryUserReadRepository
    /// Used to maintain CQRS consistency in integration tests
    /// </summary>
    public class MockUserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        private readonly ILogger<MockUserCreatedConsumer> _logger;
        private readonly IUserReadRepository _userReadRepository;

        public MockUserCreatedConsumer(
            ILogger<MockUserCreatedConsumer> logger,
            IUserReadRepository userReadRepository)
        {
            _logger = logger;
            _userReadRepository = userReadRepository;
        }

        public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Mock: Processing UserCreated event for user {UserId}", message.UserId);
            
            // Synchronize read repository with write repository data
            if (_userReadRepository is InMemoryUserReadRepository inMemoryRepo)
            {
                try
                {
                    // Check if user already exists to avoid duplicates
                    var existingUser = await inMemoryRepo.GetByIdAsync(message.UserId);
                    if (existingUser == null)
                    {
                        // Create a minimal user object from the integration event data for testing
                        var user = User.Reconstitute(
                            id: message.UserId,
                            email: message.Email,
                            firstName: message.FirstName,
                            lastName: message.LastName,
                            birthdate: DateTime.UtcNow.AddYears(-25), // Default age for testing
                            phoneNumber: message.PhoneNumber,
                            latitude: 0.0, // Default coordinates for testing
                            longitude: 0.0,
                            aboutMe: "Test user",
                            passwordSalt: "test-salt",
                            passwordHash: "test-hash",
                            lastLogin: DateTime.UtcNow,
                            createdAt: DateTime.UtcNow,
                            updatedAt: null,
                            deletedAt: null,
                            isDeleted: false);
                        
                        // Add to in-memory read repository for consistency
                        InMemoryUserReadRepository.SynchronizeUser(user);
                        _logger.LogInformation("Mock: Synchronized user {UserId} to read repository", message.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Mock: Failed to synchronize user {UserId} to read repository", message.UserId);
                }
            }
        }
    }

    /// <summary>
    /// Test mock for UserLoggedInConsumer that doesn't depend on QueryDbContext
    /// Used to avoid DI resolution issues during integration tests
    /// </summary>
    public class MockUserLoggedInConsumer : IConsumer<UserLoggedInIntegrationEvent>
    {
        private readonly ILogger<MockUserLoggedInConsumer> _logger;

        public MockUserLoggedInConsumer(ILogger<MockUserLoggedInConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<UserLoggedInIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Mock: Processing UserLoggedIn event for user {UserId}", message.UserId);
            
            // Mock implementation - just log the event
            
            return Task.CompletedTask;
        }
    }
}
