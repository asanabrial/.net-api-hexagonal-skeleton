using HexagonalSkeleton.Application.IntegrationEvents;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.Consumers
{
    /// <summary>
    /// Consumer for UserCreated events
    /// Uses proper domain services for data synchronization
    /// </summary>
    public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserSyncService _userSyncService;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(
            IUserWriteRepository userWriteRepository,
            IUserSyncService userSyncService,
            ILogger<UserCreatedConsumer> logger)
        {
            _userWriteRepository = userWriteRepository;
            _userSyncService = userSyncService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Syncing user {UserId} to read model", message.UserId);

            try
            {
                // Get the complete user data from the command store
                var user = await _userWriteRepository.GetTrackedByIdAsync(message.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found in command store", message.UserId);
                    return;
                }

                // Use the sync service to properly map and sync to query store
                await _userSyncService.SyncUserAsync(user);
                _logger.LogInformation("User {UserId} successfully synced to read model", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync user {UserId} to read model", message.UserId);
                throw;
            }
        }
    }

    /// <summary>
    /// Consumer for UserLoggedIn events
    /// Updates last login time using proper domain services
    /// </summary>
    public class UserLoggedInConsumer : IConsumer<UserLoggedInIntegrationEvent>
    {
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly IUserSyncService _userSyncService;
        private readonly ILogger<UserLoggedInConsumer> _logger;

        public UserLoggedInConsumer(
            IUserWriteRepository userWriteRepository,
            IUserSyncService userSyncService,
            ILogger<UserLoggedInConsumer> logger)
        {
            _userWriteRepository = userWriteRepository;
            _userSyncService = userSyncService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserLoggedInIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Updating last login for user {UserId}", message.UserId);

            try
            {
                // Get the updated user data from command store (already has LastLogin updated)
                var user = await _userWriteRepository.GetTrackedByIdAsync(message.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found in command store", message.UserId);
                    return;
                }

                // Sync the updated user data to query store
                await _userSyncService.SyncUserAsync(user);
                _logger.LogInformation("Last login updated for user {UserId}", message.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update last login for user {UserId}", message.UserId);
                throw;
            }
        }
    }
}
