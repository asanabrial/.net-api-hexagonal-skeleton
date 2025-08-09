using AutoMapper;
using HexagonalSkeleton.Domain;
using HexagonalSkeleton.Domain.Services;
using HexagonalSkeleton.Infrastructure.Persistence.Query;
using HexagonalSkeleton.Infrastructure.Persistence.Query.Documents;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace HexagonalSkeleton.Infrastructure.Services.Sync
{
    /// <summary>
    /// Service responsible for synchronizing data from Command store (PostgreSQL) to Query store (MongoDB)
    /// Implements eventual consistency pattern in CQRS architecture
    /// </summary>
    public class UserSyncService : IUserSyncService
    {
        private readonly QueryDbContext _queryDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserSyncService> _logger;
        private readonly IMongoCollection<UserQueryDocument> _usersCollection;

        public UserSyncService(
            QueryDbContext queryDbContext,
            IMapper mapper,
            ILogger<UserSyncService> logger)
        {
            _queryDbContext = queryDbContext ?? throw new ArgumentNullException(nameof(queryDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _usersCollection = _queryDbContext.Users ?? throw new InvalidOperationException("Users collection is not initialized");
        }

        /// <summary>
        /// Synchronize user data from command store to query store
        /// Called when user aggregate is modified
        /// </summary>
        public async Task SyncUserAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting sync for user {UserId}", user.Id);

                var userDocument = _mapper.Map<UserQueryDocument>(user);
                
                // Use upsert to handle both create and update scenarios
                var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, user.Id);
                var options = new ReplaceOptions { IsUpsert = true };

                await _usersCollection.ReplaceOneAsync(filter, userDocument, options, cancellationToken);

                _logger.LogInformation("Successfully synced user {UserId} to query store", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync user {UserId} to query store", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Synchronize multiple users in batch
        /// More efficient for bulk operations
        /// </summary>
        public async Task SyncUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
        {
            var userList = users.ToList();
            if (!userList.Any())
                return;

            try
            {
                _logger.LogInformation("Starting batch sync for {UserCount} users", userList.Count);

                var bulkOps = new List<WriteModel<UserQueryDocument>>();

                foreach (var user in userList)
                {
                    var userDocument = _mapper.Map<UserQueryDocument>(user);
                    var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, user.Id);
                    var replaceOne = new ReplaceOneModel<UserQueryDocument>(filter, userDocument)
                    {
                        IsUpsert = true
                    };
                    bulkOps.Add(replaceOne);
                }

                await _usersCollection.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully batch synced {UserCount} users to query store", userList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch sync {UserCount} users to query store", userList.Count);
                throw;
            }
        }

        /// <summary>
        /// Remove user from query store (for soft deletes)
        /// </summary>
        public async Task RemoveUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Removing user {UserId} from query store", userId);

                var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, userId);
                await _usersCollection.DeleteOneAsync(filter, cancellationToken);

                _logger.LogInformation("Successfully removed user {UserId} from query store", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove user {UserId} from query store", userId);
                throw;
            }
        }

        /// <summary>
        /// Mark user as inactive (for soft deletes)
        /// Preserves data but filters out from active queries
        /// </summary>
        public async Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deactivating user {UserId} in query store", userId);

                var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, userId);
                var update = Builders<UserQueryDocument>.Update
                    .Set(u => u.IsDeleted, true)
                    .Set(u => u.DeletedAt, DateTime.UtcNow);

                await _usersCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully deactivated user {UserId} in query store", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate user {UserId} in query store", userId);
                throw;
            }
        }

        /// <summary>
        /// Update user's last login time in query store
        /// </summary>
        public async Task UpdateUserLastLoginAsync(Guid userId, DateTime loginTime, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating last login for user {UserId}", userId);

                var filter = Builders<UserQueryDocument>.Filter.Eq(u => u.Id, userId);
                var update = Builders<UserQueryDocument>.Update
                    .Set(u => u.LastLogin, loginTime)
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _usersCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

                if (result.MatchedCount == 0)
                {
                    _logger.LogWarning("User {UserId} not found in query store for login update", userId);
                }
                else
                {
                    _logger.LogInformation("Successfully updated last login for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update last login for user {UserId}", userId);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents sync status of a user in the query store
    /// </summary>
    public class UserSyncStatus
    {
        public Guid Id { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
