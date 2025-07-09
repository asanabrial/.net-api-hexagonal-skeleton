using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Infrastructure.EventHandlers
{
    /// <summary>
    /// Event handler for user logged in events
    /// Handles synchronization to query store when user login time is updated
    /// </summary>
    public class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
    {
        private readonly IUserSyncService _syncService;
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly ILogger<UserLoggedInEventHandler> _logger;

        public UserLoggedInEventHandler(
            IUserSyncService syncService,
            IUserWriteRepository userWriteRepository,
            ILogger<UserLoggedInEventHandler> logger)
        {
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _userWriteRepository = userWriteRepository ?? throw new ArgumentNullException(nameof(userWriteRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling UserLoggedInEvent for user {UserId}", notification.UserId);

                // Get the updated user from command store
                var user = await GetUserFromCommandStore(notification.UserId, cancellationToken);
                
                if (user != null)
                {
                    await _syncService.SyncUserAsync(user, cancellationToken);
                    _logger.LogInformation("Successfully synced user {UserId} login time to query store", notification.UserId);
                }
                else
                {
                    _logger.LogWarning("Could not find user {UserId} in command store for login sync", notification.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle UserLoggedInEvent for user {UserId}", notification.UserId);
                // Don't rethrow - we don't want domain events to fail the transaction
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
