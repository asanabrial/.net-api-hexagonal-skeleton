using HexagonalSkeleton.Application.Ports;
using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Domain.Ports;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Infrastructure.Services.Sync;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Infrastructure.EventHandlers.Pure
{
    /// <summary>
    /// Pure domain event handler for UserCreatedEvent
    /// No dependency on MediatR - implements our pure interface
    /// </summary>
    public class UserCreatedPureHandler : IDomainEventHandler<UserCreatedEvent>
    {
        private readonly IUserSyncService _syncService;
        private readonly IUserWriteRepository _userWriteRepository;
        private readonly ILogger<UserCreatedPureHandler> _logger;

        public UserCreatedPureHandler(
            IUserSyncService syncService,
            IUserWriteRepository userWriteRepository,
            ILogger<UserCreatedPureHandler> logger)
        {
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _userWriteRepository = userWriteRepository ?? throw new ArgumentNullException(nameof(userWriteRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(UserCreatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Handling UserCreatedEvent for user {UserId}", domainEvent.UserId);

                // Get the full user data from command store
                var user = await GetUserFromCommandStore(domainEvent.UserId, cancellationToken);
                
                if (user != null)
                {
                    await _syncService.SyncUserAsync(user, cancellationToken);
                    _logger.LogInformation("Successfully synced newly created user {UserId} to query store", domainEvent.UserId);
                }
                else
                {
                    _logger.LogWarning("Could not find user {UserId} in command store for sync", domainEvent.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle UserCreatedEvent for user {UserId}", domainEvent.UserId);
                // Don't rethrow - we don't want domain events to fail the transaction
            }
        }

        private async Task<Domain.User?> GetUserFromCommandStore(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
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
