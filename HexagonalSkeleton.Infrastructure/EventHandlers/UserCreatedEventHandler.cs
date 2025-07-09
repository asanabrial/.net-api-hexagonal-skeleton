using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Infrastructure.Services.Sync;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Infrastructure.EventHandlers
{
    /// <summary>
    /// Event handler for user created events
    /// Handles synchronization to query store when new users are created
    /// </summary>
    public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly IUserSyncService _syncService;
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly ILogger<UserCreatedEventHandler> _logger;

        public UserCreatedEventHandler(
            IUserSyncService syncService,
            IUserWriteRepository userWriteRepository,
            ILogger<UserCreatedEventHandler> logger)
        {
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _userWriteRepository = userWriteRepository ?? throw new ArgumentNullException(nameof(userWriteRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling UserCreatedEvent for user {UserId}", notification.UserId);

                // Get the full user data from command store
                // Note: We need to get this from the write repository since the event only contains partial data
                var user = await GetUserFromCommandStore(notification.UserId, cancellationToken);
                
                if (user != null)
                {
                    await _syncService.SyncUserAsync(user, cancellationToken);
                    _logger.LogInformation("Successfully synced newly created user {UserId} to query store", notification.UserId);
                }
                else
                {
                    _logger.LogWarning("Could not find user {UserId} in command store for sync", notification.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle UserCreatedEvent for user {UserId}", notification.UserId);
                // Don't rethrow - we don't want domain events to fail the transaction
                // Consider implementing retry logic or dead letter queue for failed syncs
            }
        }

        private async Task<Domain.User?> GetUserFromCommandStore(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                // Get the user from the write repository (command store)
                var user = await _userWriteRepository.GetTrackedByIdAsync(userId, cancellationToken);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user {UserId} from command store", userId);
                return null;
            }
        }
    }
}
