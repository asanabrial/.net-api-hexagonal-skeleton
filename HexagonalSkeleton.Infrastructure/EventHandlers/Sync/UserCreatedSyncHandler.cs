using HexagonalSkeleton.Domain.Events;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MongoDB.Driver;

namespace HexagonalSkeleton.Infrastructure.EventHandlers.Sync
{
    /// <summary>
    /// Synchronizes command store changes to query store
    /// Implements eventual consistency between PostgreSQL (command) and MongoDB (query)
    /// Handles UserCreatedEvent to maintain data consistency across CQRS boundaries
    /// </summary>
    public class UserCreatedSyncHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly QueryDbContext _queryContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserCreatedSyncHandler> _logger;

        public UserCreatedSyncHandler(
            QueryDbContext queryContext,
            IMapper mapper,
            ILogger<UserCreatedSyncHandler> logger)
        {
            _queryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Synchronizing user creation to query store for user ID: {UserId}", notification.UserId);

            try
            {
                // Check if user already exists in query store (idempotency)
                var existingUser = await _queryContext.Users
                    .Find(u => u.Id == notification.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingUser != null)
                {
                    _logger.LogWarning("User {UserId} already exists in query store, skipping sync", notification.UserId);
                    return;
                }

                // Create new query document from domain event
                var userQueryDocument = new UserQueryDocument
                {
                    Id = notification.UserId,
                    Email = notification.Email,
                    FullName = new FullNameDocument
                    {
                        FirstName = notification.Name,
                        LastName = notification.Surname,
                        DisplayName = $"{notification.Name} {notification.Surname}"
                    },
                    PhoneNumber = notification.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    SearchTerms = new List<string>
                    {
                        notification.Email.ToLowerInvariant(),
                        notification.Name.ToLowerInvariant(),
                        notification.Surname.ToLowerInvariant(),
                        $"{notification.Name} {notification.Surname}".ToLowerInvariant(),
                        notification.PhoneNumber
                    },
                    ProfileCompleteness = CalculateInitialProfileCompleteness(notification)
                };

                await _queryContext.Users.InsertOneAsync(userQueryDocument, cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully synchronized user creation to query store for user ID: {UserId}", notification.UserId);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                // Duplicate key error - user already exists, this is expected in some scenarios
                _logger.LogWarning("User {UserId} already exists in query store (duplicate key), skipping sync", notification.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize user creation to query store for user ID: {UserId}", notification.UserId);
                throw; // Re-throw to ensure the command side operation can handle the failure
            }
        }

        /// <summary>
        /// Calculate initial profile completeness based on available data
        /// </summary>
        private static double CalculateInitialProfileCompleteness(UserCreatedEvent userEvent)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(userEvent.Email),
                !string.IsNullOrWhiteSpace(userEvent.Name),
                !string.IsNullOrWhiteSpace(userEvent.Surname),
                !string.IsNullOrWhiteSpace(userEvent.PhoneNumber)
            };

            var completedFields = fields.Count(f => f);
            return (double)completedFields / 7 * 100; // 7 total possible fields (including location, birthdate, about)
        }
    }
}
